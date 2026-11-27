using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Application.UseCases.CajaMisteriosa;
using ejemplo.Application.UseCases.Venta;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;
using ejemplo.Infrastructure;
using ejemplo.Infrastructure.Data;

namespace ejemplo.Tests.Manual;

public sealed class OperacionesPendientesTests
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

    private static Monedero NuevoMonedero(double saldo) => new Monedero
    {
        SaldoDisponible = new Moneda { Valor = saldo, Divisa = "EUR" },
        SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" }
    };

    private static Usuario NuevoUsuario(string nombre, double saldo, InventarioPrivado inv, EstadoCuenta estado = EstadoCuenta.Activo) => new Usuario
    {
        NombreUsuario = nombre,
        Correo = nombre + "@test.com",
        Contrasena = "pwd123",
        FechaNacimiento = DateTime.UtcNow.AddYears(-25),
        Estado = estado,
        Idioma = "es",
        Monedero = NuevoMonedero(saldo),
        Inventario = inv,
        Experiencia = 0,
        Nivel = 1
    };

    private static Administrador NuevoAdmin(string nombre) => new Administrador
    {
        NombreUsuario = nombre,
        Correo = nombre + "@test.com",
        Contrasena = "pwd123",
        FechaNacimiento = DateTime.UtcNow.AddYears(-30),
        Estado = EstadoCuenta.Activo,
        Idioma = "es",
        Verificado = true,
        Monedero = NuevoMonedero(0),
        Inventario = NuevoInventario(),
        Experiencia = 0,
        Nivel = 1,
        CodigoAuditor = "AUD-" + nombre
    };

    private static ActivoDigital NuevoActivo(string nombre, InventarioPrivado inv, bool enVenta = false) => new ActivoDigital
    {
        Nombre = nombre,
        Coleccion = "Test",
        PrecioEstimado = new Moneda { Valor = 100, Divisa = "EUR" },
        Atributos = new AtributosSkin { Rareza = Rareza.Raro, Condicion = Condicion.DesgasteMinimo, FloatVal = 0.1, Patron = 1 },
        EnVenta = enVenta,
        IdentificadorUnico = Guid.NewGuid().ToString("N"),
        Inventario = inv
    };

    private static Venta NuevaVenta(Usuario vendedor, ActivoDigital activo, double precio, EstadoVenta estado = EstadoVenta.Disponible) => new Venta
    {
        PrecioVenta = new Moneda { Valor = precio, Divisa = "EUR" },
        FechaPublicacion = DateTime.UtcNow,
        EstadoVenta = estado,
        Vendedor = vendedor,
        ActivoDigital = activo
    };

    private static ResenaVendedor NuevaResena(Usuario comprador, Usuario vendedor) => new ResenaVendedor
    {
        Puntuacion = 4,
        Comentario = "Todo correcto",
        Fecha = DateTime.UtcNow,
        Oculta = false,
        Comprador = comprador,
        Vendedor = vendedor
    };

    // ---------------- Monedero / Transacción ----------------

    [Fact]
    public async Task Monedero_IncrementarSaldo_Debe_Sumar_Y_Validar_Importe_Y_Divisa()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = NuevoUsuario("Ingresos", 100, NuevoInventario());
            await uow.UsuarioRepository.SaveAsync(usuario);
            var service = new MonederoService(uow);

            await service.IncrementarSaldoAsync(usuario.Monedero.Id, new Moneda { Valor = 25.5, Divisa = "EUR" });
            Assert.Equal(125.5, usuario.Monedero.SaldoDisponible.Valor);

            await Assert.ThrowsAsync<ModelException>(() => service.IncrementarSaldoAsync(usuario.Monedero.Id, new Moneda { Valor = 0, Divisa = "EUR" }));
            await Assert.ThrowsAsync<ModelException>(() => service.IncrementarSaldoAsync(usuario.Monedero.Id, new Moneda { Valor = -5, Divisa = "EUR" }));
            await Assert.ThrowsAsync<ModelException>(() => service.IncrementarSaldoAsync(usuario.Monedero.Id, new Moneda { Valor = 5, Divisa = "USD" }));
            Assert.Equal(125.5, usuario.Monedero.SaldoDisponible.Valor);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Monedero_DecrementarSaldo_Debe_Restar_Y_No_Dejar_Saldo_Negativo()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = NuevoUsuario("Gastos", 100, NuevoInventario());
            await uow.UsuarioRepository.SaveAsync(usuario);
            var service = new MonederoService(uow);

            await service.DecrementarSaldoAsync(usuario.Monedero.Id, new Moneda { Valor = 40, Divisa = "EUR" });
            Assert.Equal(60, usuario.Monedero.SaldoDisponible.Valor);

            await Assert.ThrowsAsync<ModelException>(() => service.DecrementarSaldoAsync(usuario.Monedero.Id, new Moneda { Valor = 60.01, Divisa = "EUR" }));
            await Assert.ThrowsAsync<ModelException>(() => service.DecrementarSaldoAsync(usuario.Monedero.Id, new Moneda { Valor = -1, Divisa = "EUR" }));
            await Assert.ThrowsAsync<ModelException>(() => service.DecrementarSaldoAsync(usuario.Monedero.Id, new Moneda { Valor = 1, Divisa = "USD" }));
            Assert.Equal(60, usuario.Monedero.SaldoDisponible.Valor);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Transaccion_ValidarSaldoMonedero_Debe_Comparar_Saldo_Y_Divisa()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = NuevoUsuario("Validable", 100, NuevoInventario());
            await uow.UsuarioRepository.SaveAsync(usuario);
            var service = new TransaccionService(uow);
            var monederoId = usuario.Monedero.Id;

            Assert.True(await service.ValidarSaldoMonederoAsync(0, monederoId, new Moneda { Valor = 100, Divisa = "EUR" }));
            Assert.True(await service.ValidarSaldoMonederoAsync(0, monederoId, new Moneda { Valor = 0.5, Divisa = "EUR" }));
            Assert.False(await service.ValidarSaldoMonederoAsync(0, monederoId, new Moneda { Valor = 100.01, Divisa = "EUR" }));
            Assert.False(await service.ValidarSaldoMonederoAsync(0, monederoId, new Moneda { Valor = 1, Divisa = "USD" }));
            await Assert.ThrowsAsync<ModelException>(() => service.ValidarSaldoMonederoAsync(0, 9999, new Moneda { Valor = 1, Divisa = "EUR" }));
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ---------------- Usuario / Administrador ----------------

    [Fact]
    public async Task Usuario_SumarExperiencia_Debe_Acumular_Y_Subir_De_Nivel()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var usuario = NuevoUsuario("Jugador", 0, NuevoInventario());
            var suspendido = NuevoUsuario("Castigado", 0, NuevoInventario(), EstadoCuenta.Suspendido);
            await uow.UsuarioRepository.SaveAsync(usuario);
            await uow.UsuarioRepository.SaveAsync(suspendido);
            var service = new UsuarioService(uow);

            await service.SumarExperienciaAsync(usuario.Id, 99);
            Assert.Equal(99, usuario.Experiencia);
            Assert.Equal(1, usuario.Nivel);

            await service.SumarExperienciaAsync(usuario.Id, 1);
            Assert.Equal(100, usuario.Experiencia);
            Assert.Equal(2, usuario.Nivel);

            await service.SumarExperienciaAsync(usuario.Id, 150);
            Assert.Equal(250, usuario.Experiencia);
            Assert.Equal(3, usuario.Nivel);

            await Assert.ThrowsAsync<ModelException>(() => service.SumarExperienciaAsync(usuario.Id, 0));
            await Assert.ThrowsAsync<ModelException>(() => service.SumarExperienciaAsync(usuario.Id, -10));
            await Assert.ThrowsAsync<ModelException>(() => service.SumarExperienciaAsync(suspendido.Id, 10));
            await Assert.ThrowsAsync<ModelException>(() => service.SumarExperienciaAsync(9999, 10));
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Administrador_ActualizarEstadoKYC_Debe_Cambiar_Estado_Y_Registrar_Auditoria()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var admin = NuevoAdmin("Admin1");
            var otroAdmin = NuevoAdmin("Admin2");
            await uow.AdministradorRepository.SaveAsync(admin);
            await uow.AdministradorRepository.SaveAsync(otroAdmin);
            var usuario = NuevoUsuario("Revisado", 0, NuevoInventario());
            await uow.UsuarioRepository.SaveAsync(usuario);
            var service = new AdministradorService(uow);

            await service.ActualizarEstadoKYCAsync(admin.Id, usuario.Id, EstadoCuenta.Suspendido);
            Assert.Equal(EstadoCuenta.Suspendido, usuario.Estado);

            await service.ActualizarEstadoKYCAsync(admin.Id, usuario.Id, EstadoCuenta.Activo);
            Assert.Equal(EstadoCuenta.Activo, usuario.Estado);

            var registros = await uow.RegistroAuditoriaRepository.ReadAllAsync();
            Assert.Equal(2, registros.Count);
            Assert.All(registros, r => Assert.Equal(admin.Id, r.Administrador.Id));
            Assert.Contains(registros, r => r.Accion.Contains("Suspendido"));

            // Mismo estado, su propia cuenta u otro administrador: rechazado y sin nuevos registros.
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(admin.Id, usuario.Id, EstadoCuenta.Activo));
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(admin.Id, admin.Id, EstadoCuenta.Suspendido));
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(admin.Id, otroAdmin.Id, EstadoCuenta.Suspendido));
            // Un usuario normal no puede actuar como administrador.
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(usuario.Id, otroAdmin.Id, EstadoCuenta.Suspendido));
            Assert.Equal(2, (await uow.RegistroAuditoriaRepository.ReadAllAsync()).Count);
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ---------------- Venta ----------------

    [Fact]
    public async Task Venta_PublicarVenta_Debe_Poner_En_Venta_Y_Dar_Experiencia()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var invVendedor = NuevoInventario();
            var vendedor = NuevoUsuario("Vendedor", 0, invVendedor);
            await uow.UsuarioRepository.SaveAsync(vendedor);
            var activo = NuevoActivo("SkinPropia", invVendedor);
            await uow.ActivoDigitalRepository.SaveAsync(activo);

            var venta = NuevaVenta(vendedor, activo, 50);
            await uow.VentaRepository.SaveAsync(venta);

            var service = new VentaService(uow);
            await service.PublicarVentaAsync(venta.Id, activo.Id);

            Assert.True(activo.EnVenta);
            Assert.Equal(activo.Id, venta.ActivoDigital.Id);
            Assert.Equal(EstadoVenta.Disponible, venta.EstadoVenta);
            Assert.Equal(10, vendedor.Experiencia);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Venta_PublicarVenta_Debe_Rechazar_Activo_Ajeno_O_Venta_No_Disponible()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var invVendedor = NuevoInventario();
            var vendedor = NuevoUsuario("VendedorA", 0, invVendedor);
            var invOtro = NuevoInventario();
            var otro = NuevoUsuario("OtroB", 0, invOtro);
            await uow.UsuarioRepository.SaveAsync(vendedor);
            await uow.UsuarioRepository.SaveAsync(otro);

            var propio = NuevoActivo("Propio", invVendedor);
            var ajeno = NuevoActivo("Ajeno", invOtro);
            var propio2 = NuevoActivo("Propio2", invVendedor);
            await uow.ActivoDigitalRepository.SaveAsync(propio);
            await uow.ActivoDigitalRepository.SaveAsync(ajeno);
            await uow.ActivoDigitalRepository.SaveAsync(propio2);

            var venta = NuevaVenta(vendedor, propio, 50);
            var completada = NuevaVenta(vendedor, propio2, 50, EstadoVenta.Completada);
            await uow.VentaRepository.SaveAsync(venta);
            await uow.VentaRepository.SaveAsync(completada);

            var service = new VentaService(uow);
            await Assert.ThrowsAsync<ModelException>(() => service.PublicarVentaAsync(venta.Id, ajeno.Id));
            await Assert.ThrowsAsync<ModelException>(() => service.PublicarVentaAsync(completada.Id, propio2.Id));
            await Assert.ThrowsAsync<ModelException>(() => service.PublicarVentaAsync(venta.Id, 9999));
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Venta_EditarPrecio_Solo_En_Ventas_Disponibles_Y_Con_Precio_Valido()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var inv = NuevoInventario();
            var vendedor = NuevoUsuario("Editor", 0, inv);
            await uow.UsuarioRepository.SaveAsync(vendedor);
            var activo1 = NuevoActivo("A1", inv, enVenta: true);
            var activo2 = NuevoActivo("A2", inv);
            await uow.ActivoDigitalRepository.SaveAsync(activo1);
            await uow.ActivoDigitalRepository.SaveAsync(activo2);

            var disponible = NuevaVenta(vendedor, activo1, 50);
            var completada = NuevaVenta(vendedor, activo2, 50, EstadoVenta.Completada);
            await uow.VentaRepository.SaveAsync(disponible);
            await uow.VentaRepository.SaveAsync(completada);

            var service = new VentaService(uow);
            await service.EditarPrecioAsync(disponible.Id, new Moneda { Valor = 79.999, Divisa = "EUR" });
            Assert.Equal(80, disponible.PrecioVenta.Valor);
            Assert.Equal("EUR", disponible.PrecioVenta.Divisa);

            await Assert.ThrowsAsync<ModelException>(() => service.EditarPrecioAsync(disponible.Id, new Moneda { Valor = 0, Divisa = "EUR" }));
            await Assert.ThrowsAsync<ModelException>(() => service.EditarPrecioAsync(disponible.Id, new Moneda { Valor = 10, Divisa = "USD" }));
            await Assert.ThrowsAsync<ModelException>(() => service.EditarPrecioAsync(completada.Id, new Moneda { Valor = 10, Divisa = "EUR" }));
            Assert.Equal(80, disponible.PrecioVenta.Valor);
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ---------------- Reseñas ----------------

    [Fact]
    public async Task ResenaVendedor_ModerarResena_Debe_Alternar_Visibilidad_Y_Auditar()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var admin = NuevoAdmin("Moderador");
            await uow.AdministradorRepository.SaveAsync(admin);
            var comprador = NuevoUsuario("Opinador", 0, NuevoInventario());
            var vendedor = NuevoUsuario("Opinado", 0, NuevoInventario());
            await uow.UsuarioRepository.SaveAsync(comprador);
            await uow.UsuarioRepository.SaveAsync(vendedor);
            var resena = NuevaResena(comprador, vendedor);
            await uow.ResenaVendedorRepository.SaveAsync(resena);

            var service = new ResenaVendedorService(uow);
            await service.ModerarResenaAsync(resena.Id, admin.Id);
            Assert.True(resena.Oculta);
            Assert.DoesNotContain(await service.FiltrarResenasVisiblesPorVendedorAsync(vendedor.Id), r => r.Id == resena.Id);

            await service.ModerarResenaAsync(resena.Id, admin.Id);
            Assert.False(resena.Oculta);
            Assert.Contains(await service.FiltrarResenasVisiblesPorVendedorAsync(vendedor.Id), r => r.Id == resena.Id);

            Assert.Equal(2, (await uow.RegistroAuditoriaRepository.ReadAllAsync()).Count);

            // Un usuario que no es administrador no puede moderar.
            await Assert.ThrowsAsync<ModelException>(() => service.ModerarResenaAsync(resena.Id, comprador.Id));
            Assert.False(resena.Oculta);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task ResenaVendedor_OcultarResena_Debe_Ocultar_Una_Sola_Vez()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var comprador = NuevoUsuario("C", 0, NuevoInventario());
            var vendedor = NuevoUsuario("V", 0, NuevoInventario());
            await uow.UsuarioRepository.SaveAsync(comprador);
            await uow.UsuarioRepository.SaveAsync(vendedor);
            var resena = NuevaResena(comprador, vendedor);
            await uow.ResenaVendedorRepository.SaveAsync(resena);

            var service = new ResenaVendedorService(uow);
            await service.OcultarResenaAsync(resena.Id);
            Assert.True(resena.Oculta);
            await Assert.ThrowsAsync<ModelException>(() => service.OcultarResenaAsync(resena.Id));
            await Assert.ThrowsAsync<ModelException>(() => service.OcultarResenaAsync(9999));
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ---------------- Cajas ----------------

    [Fact]
    public async Task CajaMisteriosa_SimularApertura_No_Debe_Cobrar_Ni_Guardar_Nada()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var inv = NuevoInventario();
            var usuario = NuevoUsuario("Curioso", 10, inv);
            await uow.UsuarioRepository.SaveAsync(usuario);
            var activoBase = NuevoActivo("Recompensa", inv);
            await uow.ActivoDigitalRepository.SaveAsync(activoBase);

            var caja = new CajaMisteriosa { Nombre = "Caja", PrecioApertura = new Moneda { Valor = 2, Divisa = "EUR" }, Activa = true };
            await uow.CajaMisteriosaRepository.SaveAsync(caja);
            var drop = new ProbabilidadDrop { Caja = caja, ActivoRecompensa = activoBase, PorcentajeDrop = 100.0 };
            caja.Probabilidades.Add(drop);
            await uow.ProbabilidadDropRepository.SaveAsync(drop);

            var inactiva = new CajaMisteriosa { Nombre = "Cerrada", PrecioApertura = new Moneda { Valor = 2, Divisa = "EUR" }, Activa = false };
            await uow.CajaMisteriosaRepository.SaveAsync(inactiva);

            int activosAntes = (await uow.ActivoDigitalRepository.ReadAllAsync()).Count;
            int transaccionesAntes = (await uow.TransaccionRepository.ReadAllAsync()).Count;

            var service = new CajaMisteriosaService(uow);
            var resultado = await service.SimularAperturaAsync(caja.Id);

            Assert.Equal("Recompensa", resultado.Nombre);
            Assert.Equal(0, resultado.Id);
            Assert.Equal(10, usuario.Monedero.SaldoDisponible.Valor);
            Assert.Equal(activosAntes, (await uow.ActivoDigitalRepository.ReadAllAsync()).Count);
            Assert.Equal(transaccionesAntes, (await uow.TransaccionRepository.ReadAllAsync()).Count);

            await Assert.ThrowsAsync<ModelException>(() => service.SimularAperturaAsync(inactiva.Id));
            await Assert.ThrowsAsync<ModelException>(() => service.SimularAperturaAsync(9999));
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ---------------- Integración: experiencia (RF-04) ----------------

    [Fact]
    public async Task Integracion_Publicar_Comprar_Y_Abrir_Caja_Deben_Dar_Experiencia()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var invVendedor = NuevoInventario();
            var vendedor = NuevoUsuario("VendeXP", 0, invVendedor);
            var invComprador = NuevoInventario();
            var comprador = NuevoUsuario("CompraXP", 100, invComprador);
            await uow.UsuarioRepository.SaveAsync(vendedor);
            await uow.UsuarioRepository.SaveAsync(comprador);

            var activo = NuevoActivo("SkinXP", invVendedor);
            await uow.ActivoDigitalRepository.SaveAsync(activo);
            var venta = NuevaVenta(vendedor, activo, 40);
            await uow.VentaRepository.SaveAsync(venta);

            // Publicar (+10 al vendedor) y comprar (+50 comprador, +30 vendedor).
            await new VentaService(uow).PublicarVentaAsync(venta.Id, activo.Id);
            await new VentaUseCase(uow).ProcesarCompraSkinAsync(venta.Id, comprador.Id);

            Assert.Equal(EstadoVenta.Completada, venta.EstadoVenta);
            Assert.Equal(60, comprador.Monedero.SaldoDisponible.Valor);
            Assert.Equal(40, vendedor.Monedero.SaldoDisponible.Valor);
            Assert.Equal(invComprador.Id, activo.Inventario.Id);
            Assert.Equal(50, comprador.Experiencia);
            Assert.Equal(40, vendedor.Experiencia);

            // Abrir una caja (+20): el comprador llega a 70 XP.
            var caja = new CajaMisteriosa { Nombre = "CajaXP", PrecioApertura = new Moneda { Valor = 5, Divisa = "EUR" }, Activa = true };
            await uow.CajaMisteriosaRepository.SaveAsync(caja);
            var drop = new ProbabilidadDrop { Caja = caja, ActivoRecompensa = activo, PorcentajeDrop = 100.0 };
            caja.Probabilidades.Add(drop);
            await uow.ProbabilidadDropRepository.SaveAsync(drop);

            await new CajaMisteriosaUseCase(uow).AbrirCajaSorpresaAsync(caja.Id, comprador.Id);
            Assert.Equal(70, comprador.Experiencia);
            Assert.Equal(55, comprador.Monedero.SaldoDisponible.Valor);
            Assert.Equal(1, comprador.Nivel);
        }
        finally
        {
            connection.Dispose();
        }
    }
}
