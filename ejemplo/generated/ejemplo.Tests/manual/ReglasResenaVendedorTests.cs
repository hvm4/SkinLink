using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Reglas;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;
using ejemplo.Infrastructure;
using ejemplo.Infrastructure.Data;

namespace ejemplo.Tests.Manual;

// RF-18 / CU-15: solo se puede reseñar a un vendedor tras una compra completada,
// una reseña por compra, puntuación de 1 a 5 y nunca a uno mismo.
public sealed class ReglasResenaVendedorTests
{
    private static Usuario NuevoUsuario(string nombre, EstadoCuenta estado = EstadoCuenta.Activo) => new Usuario
    {
        NombreUsuario = nombre,
        Correo = nombre + "@test.com",
        Contrasena = "pwd123",
        FechaNacimiento = DateTime.UtcNow.AddYears(-25),
        Estado = estado,
        Idioma = "es",
        Monedero = new Monedero
        {
            SaldoDisponible = new Moneda { Valor = 100, Divisa = "EUR" },
            SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" }
        },
        Inventario = new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 0, Divisa = "EUR" } },
        Nivel = 1
    };

    private static ActivoDigital NuevoActivo(InventarioPrivado inv) => new ActivoDigital
    {
        Nombre = "Skin",
        Coleccion = "Test",
        PrecioEstimado = new Moneda { Valor = 10, Divisa = "EUR" },
        Atributos = new AtributosSkin { Rareza = Rareza.Comun, Condicion = Condicion.AlgoDesgastado, FloatVal = 0.2, Patron = 1 },
        IdentificadorUnico = Guid.NewGuid().ToString("N"),
        Inventario = inv
    };

    private static Venta NuevaVenta(Usuario vendedor, Usuario? comprador, ActivoDigital activo, EstadoVenta estado) => new Venta
    {
        PrecioVenta = new Moneda { Valor = 10, Divisa = "EUR" },
        FechaPublicacion = DateTime.UtcNow,
        EstadoVenta = estado,
        Vendedor = vendedor,
        Comprador = comprador,
        ActivoDigital = activo
    };

    [Fact]
    public async Task Reseña_Solo_Tras_Compra_Completada_Y_Una_Por_Compra()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ejemploDbContext>().UseSqlite(connection).Options;
        await using var db = new ejemploDbContext(options);
        db.Database.EnsureCreated();
        await using var uow = new UnitOfWork(db);

        var vendedor = NuevoUsuario("Vendedor");
        var comprador = NuevoUsuario("Comprador");
        var mirón = NuevoUsuario("Miron");
        var suspendido = NuevoUsuario("Suspendido", EstadoCuenta.Suspendido);
        foreach (var u in new[] { vendedor, comprador, mirón, suspendido })
            await uow.UsuarioRepository.SaveAsync(u);

        // Sin ninguna compra: rechazada.
        await Assert.ThrowsAsync<ModelException>(() =>
            ReglasResenaVendedor.ValidarNuevaResenaAsync(uow, comprador.Id, vendedor.Id, 5));

        // Una venta solo publicada (no completada) tampoco cuenta.
        var activo1 = NuevoActivo(vendedor.Inventario);
        await uow.ActivoDigitalRepository.SaveAsync(activo1);
        await uow.VentaRepository.SaveAsync(NuevaVenta(vendedor, comprador, activo1, EstadoVenta.Disponible));
        await Assert.ThrowsAsync<ModelException>(() =>
            ReglasResenaVendedor.ValidarNuevaResenaAsync(uow, comprador.Id, vendedor.Id, 5));

        // Tras una compra completada: permitida.
        var activo2 = NuevoActivo(comprador.Inventario);
        await uow.ActivoDigitalRepository.SaveAsync(activo2);
        await uow.VentaRepository.SaveAsync(NuevaVenta(vendedor, comprador, activo2, EstadoVenta.Completada));
        await ReglasResenaVendedor.ValidarNuevaResenaAsync(uow, comprador.Id, vendedor.Id, 4);

        // Una vez reseñada esa compra, no se puede repetir.
        await uow.ResenaVendedorRepository.SaveAsync(new ResenaVendedor
        {
            Puntuacion = 4, Comentario = "Bien", Fecha = DateTime.UtcNow, Oculta = false,
            Comprador = comprador, Vendedor = vendedor
        });
        await Assert.ThrowsAsync<ModelException>(() =>
            ReglasResenaVendedor.ValidarNuevaResenaAsync(uow, comprador.Id, vendedor.Id, 5));

        // Quien no ha comprado sigue sin poder reseñar.
        await Assert.ThrowsAsync<ModelException>(() =>
            ReglasResenaVendedor.ValidarNuevaResenaAsync(uow, mirón.Id, vendedor.Id, 5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Reseña_Con_Puntuacion_Fuera_De_Rango_O_A_Uno_Mismo_Se_Rechaza(int puntuacion)
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ejemploDbContext>().UseSqlite(connection).Options;
        await using var db = new ejemploDbContext(options);
        db.Database.EnsureCreated();
        await using var uow = new UnitOfWork(db);

        var vendedor = NuevoUsuario("V");
        var comprador = NuevoUsuario("C");
        await uow.UsuarioRepository.SaveAsync(vendedor);
        await uow.UsuarioRepository.SaveAsync(comprador);
        var activo = NuevoActivo(comprador.Inventario);
        await uow.ActivoDigitalRepository.SaveAsync(activo);
        await uow.VentaRepository.SaveAsync(NuevaVenta(vendedor, comprador, activo, EstadoVenta.Completada));

        await Assert.ThrowsAsync<ModelException>(() =>
            ReglasResenaVendedor.ValidarNuevaResenaAsync(uow, comprador.Id, vendedor.Id, puntuacion));
        await Assert.ThrowsAsync<ModelException>(() =>
            ReglasResenaVendedor.ValidarNuevaResenaAsync(uow, vendedor.Id, vendedor.Id, 5));
    }

    [Fact]
    public async Task Una_Cuenta_Suspendida_No_Puede_Reseñar()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ejemploDbContext>().UseSqlite(connection).Options;
        await using var db = new ejemploDbContext(options);
        db.Database.EnsureCreated();
        await using var uow = new UnitOfWork(db);

        var vendedor = NuevoUsuario("V2");
        var suspendido = NuevoUsuario("S2", EstadoCuenta.Suspendido);
        await uow.UsuarioRepository.SaveAsync(vendedor);
        await uow.UsuarioRepository.SaveAsync(suspendido);
        var activo = NuevoActivo(suspendido.Inventario);
        await uow.ActivoDigitalRepository.SaveAsync(activo);
        await uow.VentaRepository.SaveAsync(NuevaVenta(vendedor, suspendido, activo, EstadoVenta.Completada));

        await Assert.ThrowsAsync<ModelException>(() =>
            ReglasResenaVendedor.ValidarNuevaResenaAsync(uow, suspendido.Id, vendedor.Id, 5));
    }
}
