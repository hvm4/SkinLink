using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure;
using ejemplo.Infrastructure.Data;

namespace ejemplo.Tests.Manual;

[Trait("Category", "Database")]
public sealed class CompraventaFlujoCompletoTests
{
    // Infraestructura de test (SQLite en memoria, no toca tu BD real)
    private sealed class TestEnv : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ejemploDbContext> _options;
        private readonly List<UnitOfWork> _uows = new();

        private TestEnv(SqliteConnection connection, DbContextOptions<ejemploDbContext> options)
        {
            _connection = connection;
            _options = options;
        }

        public static async Task<TestEnv> CreateAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<ejemploDbContext>().UseSqlite(connection).Options;
            await using (var db = new ejemploDbContext(options))
            {
                await db.Database.EnsureCreatedAsync();
            }
            return new TestEnv(connection, options);
        }

        // Cada llamada = contexto nuevo (como una peticion real, sin entidades en cache).
        public UnitOfWork NewUow()
        {
            var uow = new UnitOfWork(new ejemploDbContext(_options));
            _uows.Add(uow);
            return uow;
        }

        public async Task<T> SeedAsync<T>(Func<ejemploDbContext, T> build)
        {
            await using var db = new ejemploDbContext(_options);
            T result = build(db);
            await db.SaveChangesAsync();
            return result;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var uow in _uows) await uow.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private static Moneda Eur(double v) => new Moneda { Valor = v, Divisa = "EUR" };

    private static Usuario CrearUsuario(ejemploDbContext db, string nombre, bool verificado = true)
    {
        var u = new Usuario
        {
            NombreUsuario = nombre,
            Correo = $"{nombre}@test.com",
            Contrasena = "Password123!",
            FechaNacimiento = new DateTime(2000, 1, 1),
            Estado = EstadoCuenta.Activo,
            Nivel = 1,
            Experiencia = 0,
            FechaUltimoLogin = DateTime.UtcNow,
            TemaModoOscuro = false,
            Idioma = "es",
            Verificado = verificado,
            Monedero = new Monedero { SaldoDisponible = Eur(0), SaldoPendiente = Eur(0) },
            Inventario = new InventarioPrivado { ValorEstimadoTotal = Eur(0), EsPublico = false },
        };
        db.Add(u);
        return u;
    }

    private static ActivoDigital CrearActivo(ejemploDbContext db, Usuario dueno, string nombre)
    {
        var a = new ActivoDigital
        {
            Nombre = nombre,
            Coleccion = "Test",
            PrecioEstimado = Eur(10),
            Atributos = new AtributosSkin
            {
                Rareza = Rareza.Comun,
                Condicion = Condicion.RecienFabricado,
                FloatVal = 0.1,
                Patron = 1,
            },
            EnVenta = false,
            IdentificadorUnico = Guid.NewGuid().ToString(),
            Inventario = dueno.Inventario,
        };
        db.Add(a);
        return a;
    }

    private sealed record Escenario(
        int VendedorId, int CompradorId, int TerceroId,
        int MonederoVendedorId, int MonederoCompradorId, int MonederoTerceroId,
        int InventarioVendedorId, int InventarioCompradorId,
        int ActivoId, int ActivoAjenoId, int VentaId);

    private static async Task<Escenario> SeedAsync(TestEnv env, double precio = 40)
    {
        return await env.SeedAsync(db =>
        {
            var vendedor = CrearUsuario(db, "vendedor");
            var comprador = CrearUsuario(db, "comprador");
            var tercero = CrearUsuario(db, "tercero");
            var activo = CrearActivo(db, vendedor, "skin-del-vendedor");
            var ajeno = CrearActivo(db, tercero, "skin-del-tercero");
            var venta = new Venta
            {
                PrecioVenta = Eur(precio),
                FechaPublicacion = DateTime.UtcNow,
                EstadoVenta = EstadoVenta.Disponible,
                Vendedor = vendedor,
                ActivoDigital = activo,
            };
            db.Add(venta);
            return new { vendedor, comprador, tercero, activo, ajeno, venta };
        }).ContinueWith(t =>
        {
            var x = t.Result;
            return new Escenario(
                x.vendedor.Id, x.comprador.Id, x.tercero.Id,
                x.vendedor.Monedero.Id, x.comprador.Monedero.Id, x.tercero.Monedero.Id,
                x.vendedor.Inventario.Id, x.comprador.Inventario.Id,
                x.activo.Id, x.ajeno.Id, x.venta.Id);
        });
    }

    // Ayudas de ejecución / comprobación
    private static Task<Transaccion> DepositarAsync(TestEnv env, int monederoId, double importe, string referencia) =>
        new ejemplo.Application.UseCases.Monedero.MonederoUseCase(env.NewUow())
            .DepositarFondosAsync(monederoId, Eur(importe), referencia, true);

    private static Task PublicarAsync(TestEnv env, Escenario s) =>
        new VentaService(env.NewUow()).PublicarVentaAsync(s.VentaId, s.ActivoId);

    private static Task ComprarAsync(TestEnv env, int ventaId, int compradorId) =>
        new ejemplo.Application.UseCases.Venta.VentaUseCase(env.NewUow())
            .ProcesarCompraSkinAsync(ventaId, compradorId);

    private static async Task<double> SaldoAsync(TestEnv env, int monederoId)
    {
        var monedero = await env.NewUow().MonederoRepository.ReadAsync(monederoId);
        return monedero!.SaldoDisponible.Valor;
    }

    private static async Task<Venta> LeerVentaAsync(TestEnv env, int ventaId)
    {
        var uow = env.NewUow();
        await uow.UsuarioRepository.ReadAllAsync();
        await uow.ActivoDigitalRepository.ReadAllAsync();
        return (await uow.VentaRepository.ReadAsync(ventaId))!;
    }

    private static async Task<ActivoDigital> LeerActivoAsync(TestEnv env, int activoId)
    {
        var uow = env.NewUow();
        await uow.UsuarioRepository.ReadAllAsync();
        await uow.InventarioPrivadoRepository.ReadAllAsync();
        return (await uow.ActivoDigitalRepository.ReadAsync(activoId))!;
    }

    private static async Task<List<Transaccion>> TransaccionesAsync(TestEnv env, TipoTransaccion tipo)
    {
        var uow = env.NewUow();
        await uow.MonederoRepository.ReadAllAsync();
        var todas = await uow.TransaccionRepository.ReadAllAsync();
        return todas.Where(t => t.Tipo == tipo).ToList();
    }

    private static string Detalle(Exception ex)
    {
        var partes = new List<string>();
        for (var e = ex; e is not null; e = e.InnerException!) { partes.Add(e.Message); if (e.InnerException is null) break; }
        return string.Join(" | ", partes);
    }

    // FLUJO COMPLETO: depositar → publicar → comprar → comprobar saldos
    [Fact]
    public async Task Flujo_Completo_Depositar_Publicar_Comprar_Y_Comprobar_Saldos()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env, precio: 40);

        // 1) El comprador deposita 100 EUR.
        var deposito = await DepositarAsync(env, s.MonederoCompradorId, 100, "REF-FLUJO-1");
        Assert.Equal(TipoTransaccion.Deposito, deposito.Tipo);
        Assert.Equal(100, await SaldoAsync(env, s.MonederoCompradorId), 2);

        // 2) El vendedor publica la venta: el activo pasa a estar en venta.
        await PublicarAsync(env, s);
        Assert.True((await LeerActivoAsync(env, s.ActivoId)).EnVenta);
        Assert.Equal(EstadoVenta.Disponible, (await LeerVentaAsync(env, s.VentaId)).EstadoVenta);

        // 3) El comprador compra la skin.
        await ComprarAsync(env, s.VentaId, s.CompradorId);

        // 4) Saldos: comprador 100 - 40 = 60; vendedor 0 + 40 = 40 (sin ConfiguracionSistema no hay comisión).
        Assert.Equal(60, await SaldoAsync(env, s.MonederoCompradorId), 2);
        Assert.Equal(40, await SaldoAsync(env, s.MonederoVendedorId), 2);

        // 5) La skin es ahora del comprador y ya no está en venta.
        var activo = await LeerActivoAsync(env, s.ActivoId);
        Assert.False(activo.EnVenta);
        Assert.Equal(s.InventarioCompradorId, activo.Inventario.Id);

        // 6) La venta queda completada, con comprador y las dos transacciones enlazadas.
        var venta = await LeerVentaAsync(env, s.VentaId);
        Assert.Equal(EstadoVenta.Completada, venta.EstadoVenta);
        Assert.Equal(s.CompradorId, venta.Comprador?.Id);

        // 7) Movimientos registrados: un depósito, un pago del comprador y un cobro del vendedor.
        Assert.Single(await TransaccionesAsync(env, TipoTransaccion.Deposito));
        var pagos = await TransaccionesAsync(env, TipoTransaccion.PagoVenta);
        var cobros = await TransaccionesAsync(env, TipoTransaccion.CobroVenta);
        Assert.Single(pagos);
        Assert.Single(cobros);
        Assert.Equal(40, pagos[0].Monto.Valor, 2);
        Assert.Equal(s.MonederoCompradorId, pagos[0].Monedero.Id);
        Assert.Equal(s.MonederoVendedorId, cobros[0].Monedero.Id);
    }

    [Fact]
    public async Task Compra_Con_Saldo_Insuficiente_Falla_Y_No_Cambia_Nada()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env, precio: 40);
        await DepositarAsync(env, s.MonederoCompradorId, 10, "REF-POCO-1");
        await PublicarAsync(env, s);

        var ex = await Assert.ThrowsAsync<ModelException>(() => ComprarAsync(env, s.VentaId, s.CompradorId));
        Assert.Contains("Saldo insuficiente", Detalle(ex));

        Assert.Equal(10, await SaldoAsync(env, s.MonederoCompradorId), 2);
        Assert.Equal(0, await SaldoAsync(env, s.MonederoVendedorId), 2);
        Assert.Equal(EstadoVenta.Disponible, (await LeerVentaAsync(env, s.VentaId)).EstadoVenta);
        var activo = await LeerActivoAsync(env, s.ActivoId);
        Assert.True(activo.EnVenta);
        Assert.Equal(s.InventarioVendedorId, activo.Inventario.Id);
        Assert.Empty(await TransaccionesAsync(env, TipoTransaccion.PagoVenta));
        Assert.Empty(await TransaccionesAsync(env, TipoTransaccion.CobroVenta));
    }

    [Fact]
    public async Task Una_Venta_Ya_Completada_No_Se_Puede_Comprar_Dos_Veces()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env, precio: 40);
        await DepositarAsync(env, s.MonederoCompradorId, 100, "REF-DOBLE-1");
        await DepositarAsync(env, s.MonederoTerceroId, 100, "REF-DOBLE-2");
        await PublicarAsync(env, s);

        await ComprarAsync(env, s.VentaId, s.CompradorId);
        await Assert.ThrowsAsync<ModelException>(() => ComprarAsync(env, s.VentaId, s.TerceroId));

        // Solo se movió dinero una vez.
        Assert.Equal(60, await SaldoAsync(env, s.MonederoCompradorId), 2);
        Assert.Equal(100, await SaldoAsync(env, s.MonederoTerceroId), 2);
        Assert.Equal(40, await SaldoAsync(env, s.MonederoVendedorId), 2);
        Assert.Single(await TransaccionesAsync(env, TipoTransaccion.PagoVenta));
    }

    [Fact]
    public async Task El_Vendedor_No_Puede_Comprar_Su_Propia_Venta()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env, precio: 40);
        await DepositarAsync(env, s.MonederoVendedorId, 100, "REF-PROPIA-1");
        await PublicarAsync(env, s);

        await Assert.ThrowsAsync<ModelException>(() => ComprarAsync(env, s.VentaId, s.VendedorId));
        Assert.Equal(100, await SaldoAsync(env, s.MonederoVendedorId), 2);
    }

    // depositarFondos / retirarSaldoWallet
    [Fact]
    public async Task Depositar_Sin_Confirmacion_De_Pago_No_Ingresa_Fondos()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);

        var useCase = new ejemplo.Application.UseCases.Monedero.MonederoUseCase(env.NewUow());
        await Assert.ThrowsAsync<ModelException>(() =>
            useCase.DepositarFondosAsync(s.MonederoCompradorId, Eur(50), "REF-SINCONF-1", false));

        Assert.Equal(0, await SaldoAsync(env, s.MonederoCompradorId), 2);
        Assert.Empty(await TransaccionesAsync(env, TipoTransaccion.Deposito));
    }

    [Fact]
    public async Task Depositar_Dos_Veces_La_Misma_Referencia_Falla_La_Segunda()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);

        await DepositarAsync(env, s.MonederoCompradorId, 50, "REF-REPETIDA");
        await Assert.ThrowsAsync<ModelException>(() => DepositarAsync(env, s.MonederoCompradorId, 50, "REF-REPETIDA"));

        Assert.Equal(50, await SaldoAsync(env, s.MonederoCompradorId), 2);
        Assert.Single(await TransaccionesAsync(env, TipoTransaccion.Deposito));
    }

    [Fact]
    public async Task Depositar_Con_Divisa_Distinta_A_La_Del_Monedero_Falla()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);

        var useCase = new ejemplo.Application.UseCases.Monedero.MonederoUseCase(env.NewUow());
        await Assert.ThrowsAsync<ModelException>(() =>
            useCase.DepositarFondosAsync(s.MonederoCompradorId, new Moneda { Valor = 10, Divisa = "USD" }, "REF-USD-1", true));
        Assert.Equal(0, await SaldoAsync(env, s.MonederoCompradorId), 2);
    }

    [Fact]
    public async Task Retirar_Saldo_Descuenta_Y_Registra_Una_Transaccion_De_Retiro()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);
        await DepositarAsync(env, s.MonederoCompradorId, 100, "REF-RETIRO-1");

        var useCase = new ejemplo.Application.UseCases.Monedero.MonederoUseCase(env.NewUow());
        var retiro = await useCase.RetirarSaldoWalletAsync(s.MonederoCompradorId, Eur(30), "ES00 0000 0000 0000");

        Assert.Equal(TipoTransaccion.Retiro, retiro.Tipo);
        Assert.Equal(70, await SaldoAsync(env, s.MonederoCompradorId), 2);
        Assert.Single(await TransaccionesAsync(env, TipoTransaccion.Retiro));
    }

    [Fact]
    public async Task Retirar_Mas_De_Lo_Que_Hay_Falla_Y_No_Cambia_El_Saldo()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);
        await DepositarAsync(env, s.MonederoCompradorId, 20, "REF-RETIRO-2");

        var useCase = new ejemplo.Application.UseCases.Monedero.MonederoUseCase(env.NewUow());
        var ex = await Assert.ThrowsAsync<ModelException>(() =>
            useCase.RetirarSaldoWalletAsync(s.MonederoCompradorId, Eur(500), "ES00 0000 0000 0000"));
        Assert.Contains("Saldo insuficiente", Detalle(ex));

        Assert.Equal(20, await SaldoAsync(env, s.MonederoCompradorId), 2);
        Assert.Empty(await TransaccionesAsync(env, TipoTransaccion.Retiro));
    }

    // incrementarSaldo / decrementarSaldo / validarSaldoMonedero
    [Fact]
    public async Task IncrementarSaldo_Suma_El_Importe_Al_Saldo_Disponible()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);

        await new MonederoService(env.NewUow()).IncrementarSaldoAsync(s.MonederoCompradorId, Eur(25.5));
        await new MonederoService(env.NewUow()).IncrementarSaldoAsync(s.MonederoCompradorId, Eur(4.5));

        Assert.Equal(30, await SaldoAsync(env, s.MonederoCompradorId), 2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(double.NaN)]
    public async Task IncrementarSaldo_Con_Importe_No_Valido_Lanza_ModelException(double importe)
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);

        await Assert.ThrowsAsync<ModelException>(() =>
            new MonederoService(env.NewUow()).IncrementarSaldoAsync(s.MonederoCompradorId, Eur(importe)));
        Assert.Equal(0, await SaldoAsync(env, s.MonederoCompradorId), 2);
    }

    [Fact]
    public async Task IncrementarSaldo_Con_Divisa_Distinta_Lanza_ModelException()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);

        await Assert.ThrowsAsync<ModelException>(() =>
            new MonederoService(env.NewUow()).IncrementarSaldoAsync(
                s.MonederoCompradorId, new Moneda { Valor = 10, Divisa = "USD" }));
    }

    [Fact]
    public async Task IncrementarSaldo_Con_Monedero_Inexistente_Lanza_ModelException()
    {
        await using var env = await TestEnv.CreateAsync();
        await SeedAsync(env);

        await Assert.ThrowsAsync<ModelException>(() =>
            new MonederoService(env.NewUow()).IncrementarSaldoAsync(9999, Eur(10)));
    }

    [Fact]
    public async Task DecrementarSaldo_Resta_El_Importe_Y_No_Permite_Saldo_Negativo()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);
        await new MonederoService(env.NewUow()).IncrementarSaldoAsync(s.MonederoCompradorId, Eur(50));

        await new MonederoService(env.NewUow()).DecrementarSaldoAsync(s.MonederoCompradorId, Eur(20));
        Assert.Equal(30, await SaldoAsync(env, s.MonederoCompradorId), 2);

        var ex = await Assert.ThrowsAsync<ModelException>(() =>
            new MonederoService(env.NewUow()).DecrementarSaldoAsync(s.MonederoCompradorId, Eur(31)));
        Assert.Contains("Saldo insuficiente", Detalle(ex));
        Assert.Equal(30, await SaldoAsync(env, s.MonederoCompradorId), 2);
    }

    [Fact]
    public async Task DecrementarSaldo_Con_Importe_No_Valido_Lanza_ModelException()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);
        await new MonederoService(env.NewUow()).IncrementarSaldoAsync(s.MonederoCompradorId, Eur(50));

        await Assert.ThrowsAsync<ModelException>(() =>
            new MonederoService(env.NewUow()).DecrementarSaldoAsync(s.MonederoCompradorId, Eur(-1)));
        Assert.Equal(50, await SaldoAsync(env, s.MonederoCompradorId), 2);
    }

    [Fact]
    public async Task ValidarSaldoMonedero_Devuelve_True_Si_El_Saldo_Cubre_El_Importe_Y_False_Si_No()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);
        var deposito = await DepositarAsync(env, s.MonederoCompradorId, 100, "REF-VALIDAR-1");

        var service = new TransaccionService(env.NewUow());
        Assert.True(await service.ValidarSaldoMonederoAsync(deposito.Id, s.MonederoCompradorId, Eur(100)));
        Assert.True(await service.ValidarSaldoMonederoAsync(deposito.Id, s.MonederoCompradorId, Eur(40)));
        Assert.False(await service.ValidarSaldoMonederoAsync(deposito.Id, s.MonederoCompradorId, Eur(100.01)));
        Assert.False(await service.ValidarSaldoMonederoAsync(
            deposito.Id, s.MonederoCompradorId, new Moneda { Valor = 10, Divisa = "USD" }));
    }

    [Fact]
    public async Task ValidarSaldoMonedero_Con_Monedero_Inexistente_Lanza_ModelException()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);
        var deposito = await DepositarAsync(env, s.MonederoCompradorId, 100, "REF-VALIDAR-2");

        // La implementación de validarSaldoMonedero no usa p_oid (la Transacción): solo comprueba el monedero.
        var service = new TransaccionService(env.NewUow());
        await Assert.ThrowsAsync<ModelException>(() =>
            service.ValidarSaldoMonederoAsync(deposito.Id, 9999, Eur(10)));
    }

    // publicarVenta / editarPrecio / cancelarVenta
    [Fact]
    public async Task PublicarVenta_Con_Un_Activo_Que_No_Es_De_La_Venta_Lanza_ModelException()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);

        await Assert.ThrowsAsync<ModelException>(() =>
            new VentaService(env.NewUow()).PublicarVentaAsync(s.VentaId, s.ActivoAjenoId));

        Assert.False((await LeerActivoAsync(env, s.ActivoId)).EnVenta);
        Assert.False((await LeerActivoAsync(env, s.ActivoAjenoId)).EnVenta);
    }

    [Fact]
    public async Task PublicarVenta_De_Una_Venta_Cancelada_Lanza_ModelException()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env);
        await PublicarAsync(env, s);
        await new VentaService(env.NewUow()).CancelarVentaAsync(s.VentaId);

        Assert.False((await LeerActivoAsync(env, s.ActivoId)).EnVenta);
        await Assert.ThrowsAsync<ModelException>(() => PublicarAsync(env, s));
    }

    [Fact]
    public async Task EditarPrecio_En_Una_Venta_Disponible_Cambia_El_Precio_Y_Afecta_A_La_Compra()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env, precio: 40);
        await DepositarAsync(env, s.MonederoCompradorId, 100, "REF-PRECIO-1");
        await PublicarAsync(env, s);

        await new VentaService(env.NewUow()).EditarPrecioAsync(s.VentaId, Eur(25));
        Assert.Equal(25, (await LeerVentaAsync(env, s.VentaId)).PrecioVenta.Valor, 2);

        await ComprarAsync(env, s.VentaId, s.CompradorId);
        Assert.Equal(75, await SaldoAsync(env, s.MonederoCompradorId), 2);
        Assert.Equal(25, await SaldoAsync(env, s.MonederoVendedorId), 2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task EditarPrecio_Con_Precio_No_Valido_Lanza_ModelException(double precio)
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env, precio: 40);

        await Assert.ThrowsAsync<ModelException>(() =>
            new VentaService(env.NewUow()).EditarPrecioAsync(s.VentaId, Eur(precio)));
        Assert.Equal(40, (await LeerVentaAsync(env, s.VentaId)).PrecioVenta.Valor, 2);
    }

    [Fact]
    public async Task EditarPrecio_En_Una_Venta_Completada_Lanza_ModelException()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env, precio: 40);
        await DepositarAsync(env, s.MonederoCompradorId, 100, "REF-PRECIO-2");
        await PublicarAsync(env, s);
        await ComprarAsync(env, s.VentaId, s.CompradorId);

        await Assert.ThrowsAsync<ModelException>(() =>
            new VentaService(env.NewUow()).EditarPrecioAsync(s.VentaId, Eur(5)));
        Assert.Equal(40, (await LeerVentaAsync(env, s.VentaId)).PrecioVenta.Valor, 2);
    }

    [Fact]
    public async Task CancelarVenta_Libera_El_Activo_Y_Ya_No_Se_Puede_Comprar()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedAsync(env, precio: 40);
        await DepositarAsync(env, s.MonederoCompradorId, 100, "REF-CANCELAR-1");
        await PublicarAsync(env, s);

        await new VentaService(env.NewUow()).CancelarVentaAsync(s.VentaId);

        Assert.Equal(EstadoVenta.Cancelada, (await LeerVentaAsync(env, s.VentaId)).EstadoVenta);
        Assert.False((await LeerActivoAsync(env, s.ActivoId)).EnVenta);
        await Assert.ThrowsAsync<ModelException>(() => ComprarAsync(env, s.VentaId, s.CompradorId));
        Assert.Equal(100, await SaldoAsync(env, s.MonederoCompradorId), 2);
    }
}
