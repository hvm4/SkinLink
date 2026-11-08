using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Application.UseCases.Intercambio;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure;
using ejemplo.Infrastructure.Data;

namespace ejemplo.Tests.Manual;

[Trait("Category", "Database")]
public sealed class BlancaOperationsTests
{
    // ------------------------------------------------------------------
    // Infraestructura de test (SQLite en memoria, no toca tu BD real)
    // ------------------------------------------------------------------
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

    private static Usuario CrearUsuario(ejemploDbContext db, string nombre, bool inventarioPublico = false)
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
            Monedero = new Monedero { SaldoDisponible = Eur(0), SaldoPendiente = Eur(0) },
            Inventario = new InventarioPrivado { ValorEstimadoTotal = Eur(0), EsPublico = inventarioPublico },
        };
        db.Add(u);
        return u;
    }

    private static ActivoDigital CrearActivo(ejemploDbContext db, Usuario dueno, string nombre,
        bool enVenta = false, string? identificador = null)
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
            EnVenta = enVenta,
            IdentificadorUnico = identificador ?? Guid.NewGuid().ToString(),
            Inventario = dueno.Inventario,
        };
        db.Add(a);
        return a;
    }

    private static Venta CrearVenta(ejemploDbContext db, Usuario vendedor, double precio, EstadoVenta estado)
    {
        ActivoDigital activo = CrearActivo(db, vendedor, $"skin-venta-{precio}-{estado}", enVenta: true);
        var v = new Venta
        {
            PrecioVenta = Eur(precio),
            FechaPublicacion = DateTime.UtcNow,
            EstadoVenta = estado,
            Vendedor = vendedor,
            ActivoDigital = activo,
        };
        db.Add(v);
        return v;
    }

    private sealed record Escenario(
        int ProponenteId, int ReceptorId, int InvProponenteId, int InvReceptorId,
        int ActivoProponenteId, int ActivoReceptorId);

    // Dos usuarios, cada uno con un activo en su inventario.
    private static async Task<Escenario> SeedTradeAsync(TestEnv env)
    {
        return await env.SeedAsync(db =>
        {
            var p = CrearUsuario(db, "proponente");
            var r = CrearUsuario(db, "receptor");
            var ap = CrearActivo(db, p, "skin-proponente");
            var ar = CrearActivo(db, r, "skin-receptor");
            return new { p, r, ap, ar };
        }).ContinueWith(t =>
        {
            var x = t.Result;
            return new Escenario(x.p.Id, x.r.Id, x.p.Inventario.Id, x.r.Inventario.Id, x.ap.Id, x.ar.Id);
        });
    }

    // Carga en orden para que EF enlace las navegaciones antes de comprobar.
    private static async Task<IReadOnlyList<ActivoDigital>> LoadActivosAsync(UnitOfWork uow)
    {
        await uow.UsuarioRepository.ReadAllAsync();
        await uow.InventarioPrivadoRepository.ReadAllAsync();
        await uow.IntercambioRepository.ReadAllAsync();
        return await uow.ActivoDigitalRepository.ReadAllAsync();
    }

    // ------------------------------------------------------------------
    // CUSTOM: marcarComoFavorito
    // ------------------------------------------------------------------
    [Fact]
    public async Task MarcarComoFavorito_Alterna_Anadir_Y_Retirar()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);

        var service = new ActivoDigitalService(env.NewUow());
        Assert.True(await service.MarcarComoFavoritoAsync(s.ActivoReceptorId, s.ProponenteId));
        Assert.Single(await env.NewUow().FavoritoRepository.ReadAllAsync());

        var service2 = new ActivoDigitalService(env.NewUow());
        Assert.False(await service2.MarcarComoFavoritoAsync(s.ActivoReceptorId, s.ProponenteId));
        Assert.Empty(await env.NewUow().FavoritoRepository.ReadAllAsync());
    }

    [Fact]
    public async Task MarcarComoFavorito_Con_Activo_Inexistente_Lanza_ModelException()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);

        var service = new ActivoDigitalService(env.NewUow());
        await Assert.ThrowsAsync<ModelException>(() => service.MarcarComoFavoritoAsync(9999, s.ProponenteId));
    }

    // ------------------------------------------------------------------
    // CUSTOM: verificarAutenticidad
    // ------------------------------------------------------------------
    [Fact]
    public async Task VerificarAutenticidad_Guid_Valido_Devuelve_True()
    {
        await using var env = await TestEnv.CreateAsync();
        int id = await env.SeedAsync(db => { var u = CrearUsuario(db, "u1"); return CrearActivo(db, u, "a"); })
            .ContinueWith(t => t.Result.Id);

        var service = new ActivoDigitalService(env.NewUow());
        Assert.True(await service.VerificarAutenticidadAsync(id));
    }

    [Fact]
    public async Task VerificarAutenticidad_Identificador_No_Guid_Devuelve_False()
    {
        await using var env = await TestEnv.CreateAsync();
        int id = await env.SeedAsync(db =>
        {
            var u = CrearUsuario(db, "u1");
            return CrearActivo(db, u, "a", identificador: "esto-no-es-un-guid");
        }).ContinueWith(t => t.Result.Id);

        var service = new ActivoDigitalService(env.NewUow());
        Assert.False(await service.VerificarAutenticidadAsync(id));
    }

    // ------------------------------------------------------------------
    // CUSTOM: cambiarVisibilidadInventario
    // ------------------------------------------------------------------
    [Fact]
    public async Task CambiarVisibilidadInventario_Alterna_Publico_Privado()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);   // inventarios privados (EsPublico = false)

        Assert.True(await new InventarioPrivadoService(env.NewUow()).CambiarVisibilidadInventarioAsync(s.InvProponenteId));
        var tras1 = await new InventarioPrivadoService(env.NewUow()).ReadOIDAsync(s.InvProponenteId);
        Assert.True(tras1!.EsPublico);

        Assert.False(await new InventarioPrivadoService(env.NewUow()).CambiarVisibilidadInventarioAsync(s.InvProponenteId));
        var tras2 = await new InventarioPrivadoService(env.NewUow()).ReadOIDAsync(s.InvProponenteId);
        Assert.False(tras2!.EsPublico);
    }

    // ------------------------------------------------------------------
    // CUSTOMTRANSACTION: iniciarIntercambio
    // ------------------------------------------------------------------
    [Fact]
    public async Task IniciarIntercambio_Crea_Propuesta_Pendiente_Y_Reserva_Los_Activos()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);

        var service = new IntercambioUseCase(env.NewUow());
        Intercambio creado = await service.IniciarIntercambioAsync(
            s.ProponenteId, s.ReceptorId,
            new List<int> { s.ActivoProponenteId }, new List<int> { s.ActivoReceptorId });

        Assert.Equal(EstadoIntercambio.Pendiente, creado.Estado);

        var activosFrescos = await LoadActivosAsync(env.NewUow());
        Assert.Equal(creado.Id, activosFrescos.Single(a => a.Id == s.ActivoProponenteId).IntercambioOfrecido?.Id);
        Assert.Equal(creado.Id, activosFrescos.Single(a => a.Id == s.ActivoReceptorId).IntercambioSolicitado?.Id);
    }

    [Fact]
    public async Task IniciarIntercambio_Con_Activo_Ajeno_Lanza_Y_No_Crea_Nada()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);

        var service = new IntercambioUseCase(env.NewUow());
        // El proponente "ofrece" el activo del receptor: no es suyo.
        await Assert.ThrowsAsync<ModelException>(() => service.IniciarIntercambioAsync(
            s.ProponenteId, s.ReceptorId,
            new List<int> { s.ActivoReceptorId }, new List<int> { s.ActivoReceptorId }));

        Assert.Empty(await env.NewUow().IntercambioRepository.ReadAllAsync());
    }

    [Fact]
    public async Task IniciarIntercambio_Con_Uno_Mismo_Lanza_ModelException()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);

        var service = new IntercambioUseCase(env.NewUow());
        await Assert.ThrowsAsync<ModelException>(() => service.IniciarIntercambioAsync(
            s.ProponenteId, s.ProponenteId,
            new List<int> { s.ActivoProponenteId }, new List<int> { s.ActivoReceptorId }));
    }

    [Fact]
    public async Task IniciarIntercambio_Con_Activo_Solicitado_En_Venta_Lanza_Y_No_Crea_Nada()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);

        // El receptor publica su activo en venta: ya no puede intercambiarse.
        await env.SeedAsync(db =>
        {
            db.Set<ActivoDigital>().Single(a => a.Id == s.ActivoReceptorId).EnVenta = true;
            return 0;
        });

        var service = new IntercambioUseCase(env.NewUow());
        await Assert.ThrowsAsync<ModelException>(() => service.IniciarIntercambioAsync(
            s.ProponenteId, s.ReceptorId,
            new List<int> { s.ActivoProponenteId }, new List<int> { s.ActivoReceptorId }));

        Assert.Empty(await env.NewUow().IntercambioRepository.ReadAllAsync());
    }

    // ------------------------------------------------------------------
    // CUSTOMTRANSACTION: aceptarIntercambio
    // ------------------------------------------------------------------
    [Fact]
    public async Task AceptarIntercambio_Intercambia_La_Propiedad_De_Los_Activos()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);
        Intercambio inter = await new IntercambioUseCase(env.NewUow()).IniciarIntercambioAsync(
            s.ProponenteId, s.ReceptorId,
            new List<int> { s.ActivoProponenteId }, new List<int> { s.ActivoReceptorId });

        await new IntercambioUseCase(env.NewUow()).AceptarIntercambioAsync(inter.Id, s.ReceptorId);

        var uow = env.NewUow();
        var activos = await LoadActivosAsync(uow);
        Assert.Equal(s.InvReceptorId, activos.Single(a => a.Id == s.ActivoProponenteId).Inventario.Id);
        Assert.Equal(s.InvProponenteId, activos.Single(a => a.Id == s.ActivoReceptorId).Inventario.Id);
        Assert.Null(activos.Single(a => a.Id == s.ActivoProponenteId).IntercambioOfrecido);

        var guardado = await new IntercambioService(env.NewUow()).ReadOIDAsync(inter.Id);
        Assert.Equal(EstadoIntercambio.Aceptado, guardado!.Estado);
    }

    [Fact]
    public async Task AceptarIntercambio_Por_Quien_No_Es_Receptor_Lanza_Y_No_Cambia_Nada()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);
        Intercambio inter = await new IntercambioUseCase(env.NewUow()).IniciarIntercambioAsync(
            s.ProponenteId, s.ReceptorId,
            new List<int> { s.ActivoProponenteId }, new List<int> { s.ActivoReceptorId });

        await Assert.ThrowsAsync<ModelException>(() =>
            new IntercambioUseCase(env.NewUow()).AceptarIntercambioAsync(inter.Id, s.ProponenteId));

        var activos = await LoadActivosAsync(env.NewUow());
        Assert.Equal(s.InvProponenteId, activos.Single(a => a.Id == s.ActivoProponenteId).Inventario.Id);
        var guardado = await new IntercambioService(env.NewUow()).ReadOIDAsync(inter.Id);
        Assert.Equal(EstadoIntercambio.Pendiente, guardado!.Estado);
    }

    [Fact]
    public async Task AceptarIntercambio_Dos_Veces_Lanza_La_Segunda()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);
        Intercambio inter = await new IntercambioUseCase(env.NewUow()).IniciarIntercambioAsync(
            s.ProponenteId, s.ReceptorId,
            new List<int> { s.ActivoProponenteId }, new List<int> { s.ActivoReceptorId });

        await new IntercambioUseCase(env.NewUow()).AceptarIntercambioAsync(inter.Id, s.ReceptorId);
        await Assert.ThrowsAsync<ModelException>(() =>
            new IntercambioUseCase(env.NewUow()).AceptarIntercambioAsync(inter.Id, s.ReceptorId));
    }

    [Fact]
    public async Task AceptarIntercambio_Si_Un_Activo_Ya_No_Es_Del_Dueno_No_Se_Ejecuta_Nada()
    {
        await using var env = await TestEnv.CreateAsync();
        var s = await SeedTradeAsync(env);
        Intercambio inter = await new IntercambioUseCase(env.NewUow()).IniciarIntercambioAsync(
            s.ProponenteId, s.ReceptorId,
            new List<int> { s.ActivoProponenteId }, new List<int> { s.ActivoReceptorId });

        // Entre medias, el activo del receptor se publica en venta -> el trato ya no es valido.
        await env.SeedAsync(db =>
        {
            var activo = db.Set<ActivoDigital>().Single(a => a.Id == s.ActivoReceptorId);
            activo.EnVenta = true;
            return 0;
        });

        await Assert.ThrowsAsync<ModelException>(() =>
            new IntercambioUseCase(env.NewUow()).AceptarIntercambioAsync(inter.Id, s.ReceptorId));

        // Atomicidad: el activo ofrecido NO ha cambiado de dueno.
        var activos = await LoadActivosAsync(env.NewUow());
        Assert.Equal(s.InvProponenteId, activos.Single(a => a.Id == s.ActivoProponenteId).Inventario.Id);
    }

    // ------------------------------------------------------------------
    // READFILTER: filtrarVentasPorEstado / PorPrecioMaximo / filtrarActivosEnVenta
    // ------------------------------------------------------------------
    [Fact]
    public async Task FiltrarVentasPorEstado_Devuelve_Solo_Las_Del_Estado()
    {
        await using var env = await TestEnv.CreateAsync();
        await env.SeedAsync(db =>
        {
            var u = CrearUsuario(db, "vendedor");
            CrearVenta(db, u, 10, EstadoVenta.Disponible);
            CrearVenta(db, u, 20, EstadoVenta.Disponible);
            CrearVenta(db, u, 30, EstadoVenta.Completada);
            CrearVenta(db, u, 40, EstadoVenta.Cancelada);
            return 0;
        });

        var service = new VentaService(env.NewUow());
        var disponibles = await service.FiltrarVentasPorEstadoAsync(EstadoVenta.Disponible);
        var enProceso = await service.FiltrarVentasPorEstadoAsync(EstadoVenta.EnProceso);

        Assert.Equal(2, disponibles.Count);
        Assert.All(disponibles, v => Assert.Equal(EstadoVenta.Disponible, v.EstadoVenta));
        Assert.Empty(enProceso);
    }

    [Fact]
    public async Task FiltrarVentasPorPrecioMaximo_Incluye_El_Limite_Y_Excluye_Los_Mayores()
    {
        await using var env = await TestEnv.CreateAsync();
        await env.SeedAsync(db =>
        {
            var u = CrearUsuario(db, "vendedor");
            CrearVenta(db, u, 10, EstadoVenta.Disponible);
            CrearVenta(db, u, 50, EstadoVenta.Disponible);
            CrearVenta(db, u, 100, EstadoVenta.Disponible);
            return 0;
        });

        var service = new VentaService(env.NewUow());
        var resultado = await service.FiltrarVentasPorPrecioMaximoAsync(50);

        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, v => Assert.True(v.PrecioVenta.Valor <= 50));
    }

    [Fact]
    public async Task FiltrarActivosEnVenta_Separa_Publicados_Y_Guardados_Por_Usuario()
    {
        await using var env = await TestEnv.CreateAsync();
        var ids = await env.SeedAsync(db =>
        {
            var a = CrearUsuario(db, "usuarioA");
            var b = CrearUsuario(db, "usuarioB");
            CrearActivo(db, a, "a-venta-1", enVenta: true);
            CrearActivo(db, a, "a-venta-2", enVenta: true);
            CrearActivo(db, a, "a-guardado", enVenta: false);
            CrearActivo(db, b, "b-venta", enVenta: true);
            return a;
        });

        var service = new ActivoDigitalService(env.NewUow());
        var enVenta = await service.FiltrarActivosEnVentaAsync(ids.Id, true);
        var guardados = await service.FiltrarActivosEnVentaAsync(ids.Id, false);

        Assert.Equal(2, enVenta.Count);
        Assert.Single(guardados);
        Assert.Equal("a-guardado", guardados[0].Nombre);
    }
}