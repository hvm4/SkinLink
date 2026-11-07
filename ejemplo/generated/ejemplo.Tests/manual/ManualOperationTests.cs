using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using ejemplo.Application.Services;
using ejemplo.Application.UseCases.Venta;
using ejemplo.Application.UseCases.Usuario;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;
using ejemplo.Infrastructure;
using ejemplo.Infrastructure.Data;

namespace ejemplo.Tests.Manual;

public sealed class ManualOperationTests
{
    private readonly ITestOutputHelper _output;

    public ManualOperationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private const string SuccessColor = "\u001b[32m";
    private const string ErrorColor = "\u001b[31m";
    private const string ResetColor = "\u001b[0m";

    private void Ensure(bool condition, string message)
    {
        WriteTestMessage(condition, message);
        Assert.True(condition, message);
    }

    private void WriteTestMessage(bool condition, string message)
    {
        var color = condition ? SuccessColor : ErrorColor;
        var status = condition ? "[OK] " : "[FAIL] ";
        _output.WriteLine($"{color}{status}{message}{ResetColor}");
    }

    private static (SqliteConnection connection, DbContextOptions<ejemploDbContext> options) CreateInMemorySqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ejemploDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var db = new ejemploDbContext(options))
        {
            db.Database.EnsureCreated();
        }

        return (connection, options);
    }

    [Fact]
    public async Task ActivoDigital_ActualizarPrecio_Debe_Actualizar_El_Precio_Estimado()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var inventario = new InventarioPrivado
            {
                ValorEstimadoTotal = new Moneda { Valor = 100, Divisa = "EUR" },
                EsPublico = true
            };
            await uow.InventarioPrivadoRepository.SaveAsync(inventario);

            var activo = new ActivoDigital
            {
                Nombre = "AK-47 | Asiimov",
                Coleccion = "Phoenix",
                PrecioEstimado = new Moneda { Valor = 50.0, Divisa = "EUR" },
                Atributos = new AtributosSkin { Rareza = Rareza.Extraordinario, Condicion = Condicion.RecienFabricado, FloatVal = 0.05, Patron = 12 },
                EnVenta = false,
                IdentificadorUnico = Guid.NewGuid().ToString("N"),
                Inventario = inventario
            };
            await uow.ActivoDigitalRepository.SaveAsync(activo);
            await uow.CommitAsync();

            var service = new ActivoDigitalService(uow);
            await service.ActualizarPrecioAsync(activo.Id, new Moneda { Valor = 75.5, Divisa = "EUR" });

            var activoActualizado = await uow.ActivoDigitalRepository.ReadAsync(activo.Id);
            Ensure(activoActualizado != null, "ActivoDigital encontrado tras actualizar precio.");
            Ensure(activoActualizado!.PrecioEstimado.Valor == 75.5, "Precio actualizado correctamente.");
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Venta_ProcesarCompraSkin_Debe_Transferir_Activo_Y_Saldos()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            // Vendedor
            var monederoVendedor = new Monedero { SaldoDisponible = new Moneda { Valor = 0, Divisa = "EUR" }, SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" } };
            var invVendedor = new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 100, Divisa = "EUR" }, EsPublico = true };
            var vendedor = new Usuario
            {
                NombreUsuario = "VendedorTest",
                Correo = "vendedor@test.com",
                Contrasena = "pwd123",
                FechaNacimiento = DateTime.UtcNow.AddYears(-20),
                Estado = EstadoCuenta.Activo,
                Monedero = monederoVendedor,
                Inventario = invVendedor
            };
            await uow.UsuarioRepository.SaveAsync(vendedor);

            // Comprador
            var monederoComprador = new Monedero { SaldoDisponible = new Moneda { Valor = 200, Divisa = "EUR" }, SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" } };
            var invComprador = new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 0, Divisa = "EUR" }, EsPublico = true };
            var comprador = new Usuario
            {
                NombreUsuario = "CompradorTest",
                Correo = "comprador@test.com",
                Contrasena = "pwd123",
                FechaNacimiento = DateTime.UtcNow.AddYears(-22),
                Estado = EstadoCuenta.Activo,
                Monedero = monederoComprador,
                Inventario = invComprador,
                Experiencia = 0,
                Nivel = 1
            };
            await uow.UsuarioRepository.SaveAsync(comprador);

            // Activo y Venta
            var activo = new ActivoDigital
            {
                Nombre = "AWP | Dragon Lore",
                Coleccion = "Cobblestone",
                PrecioEstimado = new Moneda { Valor = 150, Divisa = "EUR" },
                Atributos = new AtributosSkin { Rareza = Rareza.Extraordinario, Condicion = Condicion.DesgasteMinimo, FloatVal = 0.1, Patron = 1 },
                EnVenta = true,
                IdentificadorUnico = Guid.NewGuid().ToString("N"),
                Inventario = invVendedor
            };
            await uow.ActivoDigitalRepository.SaveAsync(activo);

            var venta = new Venta
            {
                PrecioVenta = new Moneda { Valor = 150, Divisa = "EUR" },
                FechaPublicacion = DateTime.UtcNow,
                EstadoVenta = EstadoVenta.Disponible,
                Vendedor = vendedor,
                ActivoDigital = activo
            };
            await uow.VentaRepository.SaveAsync(venta);
            await uow.CommitAsync();

            // Ejecutar compra
            var ventaUseCase = new VentaUseCase(uow);
            await ventaUseCase.ProcesarCompraSkinAsync(venta.Id, comprador.Id);

            // Verificaciones
            Ensure(monederoComprador.SaldoDisponible.Valor == 50, "Saldo comprador decrementado.");
            Ensure(monederoVendedor.SaldoDisponible.Valor == 150, "Saldo vendedor incrementado.");
            Ensure(activo.Inventario.Id == invComprador.Id, "Activo transferido al inventario del comprador.");
            Ensure(!activo.EnVenta, "Activo ya no está marcado en venta.");
            Ensure(venta.EstadoVenta == EstadoVenta.Completada, "Venta marcada como completada.");
            Ensure(comprador.Experiencia > 0, "Comprador recibió experiencia tras la compra.");
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Usuario_ReclamarRecompensaDiaria_Debe_Entregar_Skin_Y_Actualizar_Fecha()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);

            var inv = new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 0, Divisa = "EUR" }, EsPublico = true };
            var usuario = new Usuario
            {
                NombreUsuario = "UsuarioDiario",
                Correo = "diario@test.com",
                Contrasena = "pwd123",
                FechaNacimiento = DateTime.UtcNow.AddYears(-25),
                Estado = EstadoCuenta.Activo,
                Monedero = new Monedero { SaldoDisponible = new Moneda { Valor = 10, Divisa = "EUR" }, SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" } },
                Inventario = inv,
                FechaUltimaRecompensa = null,
                Experiencia = 0,
                Nivel = 1
            };
            await uow.UsuarioRepository.SaveAsync(usuario);

            var activoBase = new ActivoDigital
            {
                Nombre = "M4A4 | Howl",
                Coleccion = "Huntsman",
                PrecioEstimado = new Moneda { Valor = 200, Divisa = "EUR" },
                Atributos = new AtributosSkin { Rareza = Rareza.Extraordinario, Condicion = Condicion.RecienFabricado, FloatVal = 0.03, Patron = 5 },
                EnVenta = false,
                IdentificadorUnico = Guid.NewGuid().ToString("N"),
                Inventario = inv
            };
            await uow.ActivoDigitalRepository.SaveAsync(activoBase);

            var caja = new CajaMisteriosa
            {
                Nombre = "Caja Diaria Gratuita",
                PrecioApertura = new Moneda { Valor = 0, Divisa = "EUR" },
                Activa = true
            };
            await uow.CajaMisteriosaRepository.SaveAsync(caja);

            var drop = new ProbabilidadDrop
            {
                Caja = caja,
                ActivoRecompensa = activoBase,
                PorcentajeDrop = 100.0
            };
            caja.Probabilidades.Add(drop);
            await uow.ProbabilidadDropRepository.SaveAsync(drop);
            await uow.CommitAsync();

            var usuarioUseCase = new UsuarioUseCase(uow);
            var activoObtenido = await usuarioUseCase.ReclamarRecompensaDiariaAsync(usuario.Id, caja.Id);

            Ensure(activoObtenido != null, "Se obtuvo un activo digital de la recompensa.");
            Ensure(activoObtenido!.Nombre == "M4A4 | Howl", "El activo obtenido coincide con el drop configurado.");
            Ensure(usuario.FechaUltimaRecompensa.HasValue, "Fecha de última recompensa actualizada.");
            Ensure(usuario.Experiencia == 25, "Experiencia diaria añadida.");
        }
        finally
        {
            connection.Dispose();
        }
    }
}
