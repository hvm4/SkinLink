using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Application.UseCases.CajaMisteriosa;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;
using ejemplo.Infrastructure;
using ejemplo.Infrastructure.Data;

namespace ejemplo.Tests.Manual;

// Tests de suspender cuentas, descuentos de catálogo, apertura de cajas, bloquear/desbloquear activos
// y comportamiento real de los ReadFilter de Usuario, Transacción y ActivoDigital (rareza, colección, condición).
// Las operaciones de Blanca (favoritos, visibilidad, autenticidad, intercambios, filtros de Venta y activos en venta) están en BlancaOperationsTests.
public sealed class ManualOperationTestsCuentasDescuentosCajasBloqueoYFiltros
{
    private static (SqliteConnection connection, DbContextOptions<ejemploDbContext> options) CreateInMemorySqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ejemploDbContext>().UseSqlite(connection).Options;
        using (var db = new ejemploDbContext(options))
        {
            db.Database.EnsureCreated();
        }
        return (connection, options);
    }

    private static InventarioPrivado NuevoInventario() =>
        new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 0, Divisa = "EUR" }, EsPublico = false };

    private static Usuario NuevoUsuario(string nombre, double saldo, InventarioPrivado inv, EstadoCuenta estado = EstadoCuenta.Activo) => new Usuario
    {
        NombreUsuario = nombre,
        Correo = nombre + "@test.com",
        Contrasena = "pwd123",
        FechaNacimiento = DateTime.UtcNow.AddYears(-25),
        Estado = estado,
        Monedero = new Monedero
        {
            SaldoDisponible = new Moneda { Valor = saldo, Divisa = "EUR" },
            SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" }
        },
        Inventario = inv,
        Experiencia = 0,
        Nivel = 1
    };

    private static ActivoDigital NuevoActivo(string nombre, InventarioPrivado inv, bool enVenta = false,
        Rareza rareza = Rareza.Raro, Condicion condicion = Condicion.DesgasteMinimo, string coleccion = "Test") => new ActivoDigital
    {
        Nombre = nombre,
        Coleccion = coleccion,
        PrecioEstimado = new Moneda { Valor = 100, Divisa = "EUR" },
        Atributos = new AtributosSkin { Rareza = rareza, Condicion = condicion, FloatVal = 0.1, Patron = 1 },
        EnVenta = enVenta,
        IdentificadorUnico = Guid.NewGuid().ToString("N"),
        Inventario = inv
    };

    private static Venta NuevaVenta(Usuario vendedor, ActivoDigital activo, double precio, EstadoVenta estado) => new Venta
    {
        PrecioVenta = new Moneda { Valor = precio, Divisa = "EUR" },
        FechaPublicacion = DateTime.UtcNow,
        EstadoVenta = estado,
        Vendedor = vendedor,
        ActivoDigital = activo
    };

    // ---------------- Héctor ----------------

    [Fact]
    public async Task Usuario_SuspenderCuentaUsuario_Debe_Suspender_Y_Rechazar_Repeticiones()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = NuevoUsuario("Suspendible", 0, NuevoInventario());
            await uow.UsuarioRepository.SaveAsync(usuario);

            var service = new UsuarioService(uow);
            await service.SuspenderCuentaUsuarioAsync(usuario.Id);
            Assert.Equal(EstadoCuenta.Suspendido, usuario.Estado);

            await Assert.ThrowsAsync<ModelException>(() => service.SuspenderCuentaUsuarioAsync(usuario.Id));
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task ArticuloCatalogo_ActualizarDescuentoArticulo_Debe_Validar_Porcentaje_Y_Fecha()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var articulo = new ArticuloCatalogo
            {
                Nombre = "Articulo",
                Coleccion = "Test",
                Rareza = Rareza.Raro,
                PrecioReferencia = new Moneda { Valor = 10, Divisa = "EUR" },
                Habilitado = true
            };
            await uow.ArticuloCatalogoRepository.SaveAsync(articulo);

            var service = new ArticuloCatalogoService(uow);
            var fin = DateTime.UtcNow.AddDays(7);
            await service.ActualizarDescuentoArticuloAsync(articulo.Id, 25, fin);
            Assert.Equal(25, articulo.PorcentajeDescuento);
            Assert.Equal(fin, articulo.FechaFinDescuento);

            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarDescuentoArticuloAsync(articulo.Id, 150, fin));
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarDescuentoArticuloAsync(articulo.Id, 10, DateTime.UtcNow.AddDays(-1)));
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task CajaMisteriosa_AbrirCajaSorpresa_Debe_Cobrar_Y_Entregar_El_Activo()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var inv = NuevoInventario();
            var usuario = NuevoUsuario("Abridor", 10, inv);
            await uow.UsuarioRepository.SaveAsync(usuario);
            var activoBase = NuevoActivo("Recompensa", inv);
            await uow.ActivoDigitalRepository.SaveAsync(activoBase);

            var caja = new CajaMisteriosa { Nombre = "Caja", PrecioApertura = new Moneda { Valor = 2, Divisa = "EUR" }, Activa = true };
            await uow.CajaMisteriosaRepository.SaveAsync(caja);
            var drop = new ProbabilidadDrop { Caja = caja, ActivoRecompensa = activoBase, PorcentajeDrop = 100.0 };
            caja.Probabilidades.Add(drop);
            await uow.ProbabilidadDropRepository.SaveAsync(drop);

            var useCase = new CajaMisteriosaUseCase(uow);
            var obtenido = await useCase.AbrirCajaSorpresaAsync(caja.Id, usuario.Id);

            Assert.Equal("Recompensa", obtenido.Nombre);
            Assert.Equal(inv.Id, obtenido.Inventario.Id);
            Assert.NotEqual(activoBase.Id, obtenido.Id);
            Assert.Equal(8, usuario.Monedero.SaldoDisponible.Valor);
            var transacciones = await uow.TransaccionRepository.ReadAllAsync();
            Assert.Contains(transacciones, t => t.Tipo == TipoTransaccion.CosteCaja && t.Monto.Valor == 2);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task CajaMisteriosa_AbrirCajaSorpresa_Debe_Rechazar_Saldo_Insuficiente_O_Caja_Inactiva()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var inv = NuevoInventario();
            var pobre = NuevoUsuario("Pobre", 1, inv);
            await uow.UsuarioRepository.SaveAsync(pobre);
            var activoBase = NuevoActivo("Recompensa", inv);
            await uow.ActivoDigitalRepository.SaveAsync(activoBase);

            var cara = new CajaMisteriosa { Nombre = "Cara", PrecioApertura = new Moneda { Valor = 5, Divisa = "EUR" }, Activa = true };
            var inactiva = new CajaMisteriosa { Nombre = "Inactiva", PrecioApertura = new Moneda { Valor = 0, Divisa = "EUR" }, Activa = false };
            await uow.CajaMisteriosaRepository.SaveAsync(cara);
            await uow.CajaMisteriosaRepository.SaveAsync(inactiva);
            var drop = new ProbabilidadDrop { Caja = cara, ActivoRecompensa = activoBase, PorcentajeDrop = 100.0 };
            cara.Probabilidades.Add(drop);
            await uow.ProbabilidadDropRepository.SaveAsync(drop);

            var useCase = new CajaMisteriosaUseCase(uow);
            await Assert.ThrowsAsync<ModelException>(() => useCase.AbrirCajaSorpresaAsync(cara.Id, pobre.Id));
            await Assert.ThrowsAsync<ModelException>(() => useCase.AbrirCajaSorpresaAsync(inactiva.Id, pobre.Id));
            Assert.Equal(1, pobre.Monedero.SaldoDisponible.Valor);
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ---------------- Jorge: bloquear / desbloquear ----------------

    [Fact]
    public async Task ActivoDigital_BloquearParaVenta_Y_Desbloquear_Deben_Cambiar_EnVenta()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var inv = NuevoInventario();
            var vendedor = NuevoUsuario("Bloqueador", 0, inv);
            await uow.UsuarioRepository.SaveAsync(vendedor);
            var activo = NuevoActivo("Bloqueable", inv);
            await uow.ActivoDigitalRepository.SaveAsync(activo);

            var service = new ActivoDigitalService(uow);

            await service.BloquearParaVentaAsync(activo.Id);
            Assert.True(activo.EnVenta);
            await Assert.ThrowsAsync<ModelException>(() => service.BloquearParaVentaAsync(activo.Id));

            await service.DesbloquearAsync(activo.Id);
            Assert.False(activo.EnVenta);
            await Assert.ThrowsAsync<ModelException>(() => service.DesbloquearAsync(activo.Id));
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task ActivoDigital_Desbloquear_Debe_Rechazar_Si_Hay_Una_Venta_Activa()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var inv = NuevoInventario();
            var vendedor = NuevoUsuario("VendedorActiva", 0, inv);
            await uow.UsuarioRepository.SaveAsync(vendedor);
            var activo = NuevoActivo("EnAnuncio", inv, enVenta: true);
            await uow.ActivoDigitalRepository.SaveAsync(activo);
            await uow.VentaRepository.SaveAsync(NuevaVenta(vendedor, activo, 50, EstadoVenta.Disponible));

            var service = new ActivoDigitalService(uow);
            await Assert.ThrowsAsync<ModelException>(() => service.DesbloquearAsync(activo.Id));
            Assert.True(activo.EnVenta);
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ---------------- ReadFilter: comportamiento real ----------------

    [Fact]
    public async Task Filtros_De_Usuario_Y_Transaccion_Deben_Devolver_Solo_Los_Que_Cumplen()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var activo = NuevoUsuario("FiltroActivo", 0, NuevoInventario(), EstadoCuenta.Activo);
            var suspendido = NuevoUsuario("FiltroSuspendido", 0, NuevoInventario(), EstadoCuenta.Suspendido);
            await uow.UsuarioRepository.SaveAsync(activo);
            await uow.UsuarioRepository.SaveAsync(suspendido);

            var usuarioService = new UsuarioService(uow);
            var suspendidos = await usuarioService.FiltrarUsuariosPorEstadoAsync(EstadoCuenta.Suspendido);
            Assert.Contains(suspendidos, u => u.Id == suspendido.Id);
            Assert.DoesNotContain(suspendidos, u => u.Id == activo.Id);
            Assert.All(suspendidos, u => Assert.Equal(EstadoCuenta.Suspendido, u.Estado));

            var hoy = DateTime.UtcNow;
            var deposito = new Transaccion { Tipo = TipoTransaccion.Deposito, Monto = new Moneda { Valor = 5, Divisa = "EUR" }, Fecha = hoy, Concepto = "dep", Monedero = activo.Monedero };
            var retiro = new Transaccion { Tipo = TipoTransaccion.Retiro, Monto = new Moneda { Valor = 5, Divisa = "EUR" }, Fecha = hoy.AddDays(-30), Concepto = "ret", Monedero = activo.Monedero };
            await uow.TransaccionRepository.SaveAsync(deposito);
            await uow.TransaccionRepository.SaveAsync(retiro);

            var transaccionService = new TransaccionService(uow);
            var porTipo = await transaccionService.FiltrarTransaccionesPorTipoAsync(TipoTransaccion.Deposito);
            Assert.Contains(porTipo, t => t.Id == deposito.Id);
            Assert.DoesNotContain(porTipo, t => t.Id == retiro.Id);

            var porFecha = await transaccionService.FiltrarTransaccionesPorRangoFechasAsync(hoy.AddDays(-1), hoy.AddDays(1));
            Assert.Contains(porFecha, t => t.Id == deposito.Id);
            Assert.DoesNotContain(porFecha, t => t.Id == retiro.Id);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Filtros_De_ActivoDigital_Deben_Devolver_Solo_Los_Que_Cumplen()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var inv = NuevoInventario();
            var dueno = NuevoUsuario("DuenoFiltros", 0, inv);
            await uow.UsuarioRepository.SaveAsync(dueno);

            var a1 = NuevoActivo("A1", inv, enVenta: true, rareza: Rareza.Extraordinario, condicion: Condicion.RecienFabricado, coleccion: "Alfa");
            var a2 = NuevoActivo("A2", inv, enVenta: false, rareza: Rareza.Comun, condicion: Condicion.Deplorable, coleccion: "Beta");
            await uow.ActivoDigitalRepository.SaveAsync(a1);
            await uow.ActivoDigitalRepository.SaveAsync(a2);

            var service = new ActivoDigitalService(uow);

            var porRareza = await service.FiltrarActivosPorRarezaAsync(Rareza.Extraordinario);
            Assert.Contains(porRareza, a => a.Id == a1.Id);
            Assert.DoesNotContain(porRareza, a => a.Id == a2.Id);

            var porColeccion = await service.FiltrarActivosPorColeccionAsync("Beta");
            Assert.Contains(porColeccion, a => a.Id == a2.Id);
            Assert.DoesNotContain(porColeccion, a => a.Id == a1.Id);

            var porCondicion = await service.FiltrarActivosPorCondicionAsync(Condicion.RecienFabricado);
            Assert.Contains(porCondicion, a => a.Id == a1.Id);
            Assert.DoesNotContain(porCondicion, a => a.Id == a2.Id);
        }
        finally
        {
            connection.Dispose();
        }
    }

}
