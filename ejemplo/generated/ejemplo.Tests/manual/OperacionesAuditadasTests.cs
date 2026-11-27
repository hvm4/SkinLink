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

// RF-29: suspenderCuentaUsuario y verificarUsuario ejecutados por un administrador dejan
// registro en RegistroAuditoria, en la misma transacción que la operación.
public sealed class OperacionesAuditadasTests
{
    private static Monedero Monedero() => new Monedero
    {
        SaldoDisponible = new Moneda { Valor = 0, Divisa = "EUR" },
        SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" }
    };

    private static InventarioPrivado Inventario() =>
        new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 0, Divisa = "EUR" } };

    private static Usuario NuevoUsuario(string nombre, int edad = 25) => new Usuario
    {
        NombreUsuario = nombre, Correo = nombre + "@test.com", Contrasena = "pwd123",
        FechaNacimiento = DateTime.UtcNow.AddYears(-edad), Estado = EstadoCuenta.Activo, Idioma = "es",
        Monedero = Monedero(), Inventario = Inventario(), Nivel = 1
    };

    private static Administrador NuevoAdmin(string nombre) => new Administrador
    {
        NombreUsuario = nombre, Correo = nombre + "@test.com", Contrasena = "pwd123",
        FechaNacimiento = DateTime.UtcNow.AddYears(-30), Estado = EstadoCuenta.Activo, Idioma = "es",
        Verificado = true, Monedero = Monedero(), Inventario = Inventario(), Nivel = 1, CodigoAuditor = "AUD-" + nombre
    };

    private static async Task<(SqliteConnection, ejemploDbContext, UnitOfWork)> CrearAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var db = new ejemploDbContext(new DbContextOptionsBuilder<ejemploDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        return (connection, db, new UnitOfWork(db));
    }

    [Fact]
    public async Task Suspender_Y_Verificar_Por_Un_Admin_Dejan_Registro_De_Auditoria()
    {
        var (connection, db, uow) = await CrearAsync();
        await using (db) await using (uow) using (connection)
        {
            var admin = NuevoAdmin("Admin");
            await uow.AdministradorRepository.SaveAsync(admin);
            var aVerificar = NuevoUsuario("Nuevo");
            var aSuspender = NuevoUsuario("Tramposo");
            await uow.UsuarioRepository.SaveAsync(aVerificar);
            await uow.UsuarioRepository.SaveAsync(aSuspender);

            await OperacionesAuditadas.VerificarUsuarioAsync(uow, admin.Id, aVerificar.Id);
            await OperacionesAuditadas.SuspenderCuentaUsuarioAsync(uow, admin.Id, aSuspender.Id);

            Assert.True(aVerificar.Verificado);
            Assert.Equal(EstadoCuenta.Suspendido, aSuspender.Estado);

            var registros = await uow.RegistroAuditoriaRepository.ReadAllAsync();
            Assert.Equal(2, registros.Count);
            Assert.All(registros, r => Assert.Equal(admin.Id, r.Administrador.Id));
            Assert.Contains(registros, r => r.Accion.Contains("Verificación") && r.Accion.Contains("Nuevo"));
            Assert.Contains(registros, r => r.Accion.Contains("Suspensión") && r.Accion.Contains("Tramposo"));
        }
    }

    [Fact]
    public async Task Si_La_Operacion_Falla_O_No_Es_Admin_No_Cambia_Nada_Ni_Se_Audita()
    {
        var (connection, db, uow) = await CrearAsync();
        await using (db) await using (uow) using (connection)
        {
            var admin = NuevoAdmin("Admin2");
            await uow.AdministradorRepository.SaveAsync(admin);
            var normal = NuevoUsuario("Normal");
            var menor = NuevoUsuario("Menor", edad: 15);
            await uow.UsuarioRepository.SaveAsync(normal);
            await uow.UsuarioRepository.SaveAsync(menor);

            // Un usuario normal no puede actuar como administrador.
            await Assert.ThrowsAsync<ModelException>(() =>
                OperacionesAuditadas.SuspenderCuentaUsuarioAsync(uow, normal.Id, menor.Id));
            Assert.Equal(EstadoCuenta.Activo, menor.Estado);

            // La operación de negocio falla (menor de edad): no queda verificado ni auditado.
            await Assert.ThrowsAsync<ModelException>(() =>
                OperacionesAuditadas.VerificarUsuarioAsync(uow, admin.Id, menor.Id));
            Assert.False(menor.Verificado);

            // Sobre su propia cuenta: rechazado.
            await Assert.ThrowsAsync<ModelException>(() =>
                OperacionesAuditadas.VerificarUsuarioAsync(uow, admin.Id, admin.Id));

            Assert.Empty(await uow.RegistroAuditoriaRepository.ReadAllAsync());
        }
    }
}
