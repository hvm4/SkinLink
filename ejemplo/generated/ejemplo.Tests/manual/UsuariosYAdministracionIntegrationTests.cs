using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;
using ejemplo.Infrastructure;
using ejemplo.Infrastructure.Data;

namespace ejemplo.Tests.Manual;

// Miembro 1 · Prueba de integración: base de datos SQLite en fichero, un UnitOfWork (DbContext) nuevo
// en cada paso, como ocurre en la web con cada petición. Recorre el ciclo de vida de una cuenta:
// alta (Usuario y Administrador) -> login -> verificación -> experiencia -> KYC -> auditoría -> filtro.
public sealed class UsuariosYAdministracionIntegrationTests
{
    private static Monedero NuevoMonedero() => new Monedero
    {
        SaldoDisponible = new Moneda { Valor = 0, Divisa = "EUR" },
        SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" }
    };

    private static InventarioPrivado NuevoInventario() =>
        new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 0, Divisa = "EUR" }, EsPublico = false };

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Ciclo_De_Vida_De_Una_Cuenta_Con_Auditoria_En_Base_De_Datos()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"skinlink-miembro1-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<ejemploDbContext>().UseSqlite($"Data Source={dbPath}").Options;
        try
        {
            await using (var db = new ejemploDbContext(options))
            {
                await db.Database.EnsureCreatedAsync();
            }

            // 1) Alta de un administrador y de un usuario mayor de edad con los servicios (contraseñas hasheadas).
            int adminId, usuarioId;
            await using (var db = new ejemploDbContext(options))
            await using (var uow = new UnitOfWork(db))
            {
                var admin = await new AdministradorService(uow).NewAsync("admin", "admin@skinlink.com", "Admin123!",
                    new DateTime(1990, 5, 1), EstadoCuenta.Activo, 1, 0, DateTime.UtcNow, false, "es", true,
                    NuevoMonedero(), NuevoInventario(), "AUD-001");
                var usuario = await new UsuarioService(uow).NewAsync("jugador", "jugador@test.com", "Jugador123!",
                    new DateTime(2000, 1, 1), EstadoCuenta.Activo, 1, 0, DateTime.UtcNow, false, "es", false,
                    NuevoMonedero(), NuevoInventario());
                adminId = admin.Id;
                usuarioId = usuario.Id;
                Assert.NotEqual("Jugador123!", usuario.Contrasena);
            }

            // 2) Login por correo de los dos (el administrador entra como Usuario: herencia).
            await using (var db = new ejemploDbContext(options))
            await using (var uow = new UnitOfWork(db))
            {
                var service = new UsuarioService(uow);
                Assert.False(string.IsNullOrWhiteSpace(await service.LoginAsync("admin@skinlink.com", "Admin123!")));
                Assert.False(string.IsNullOrWhiteSpace(await service.LoginAsync("jugador@test.com", "Jugador123!")));
                Assert.Null(await service.LoginAsync("jugador@test.com", "contraseña-incorrecta"));
                Assert.Null(await service.LoginAsync("noexiste@test.com", "Jugador123!"));
                Assert.Null(await service.LoginAsync("", "Jugador123!"));
                Assert.IsType<Administrador>(await service.ReadOIDAsync(adminId));
            }

            // 3) Verificación de identidad y experiencia.
            await using (var db = new ejemploDbContext(options))
            await using (var uow = new UnitOfWork(db))
            {
                var service = new UsuarioService(uow);
                await service.VerificarUsuarioAsync(usuarioId);
                await service.SumarExperienciaAsync(usuarioId, 250);
            }
            await using (var db = new ejemploDbContext(options))
            await using (var uow = new UnitOfWork(db))
            {
                var usuario = await uow.UsuarioRepository.ReadAsync(usuarioId);
                Assert.NotNull(usuario);
                Assert.True(usuario!.Verificado);
                Assert.Equal(250, usuario.Experiencia);
                Assert.Equal(3, usuario.Nivel);
            }

            // 4) El administrador suspende la cuenta (KYC): queda persistido junto con su registro de auditoría.
            await using (var db = new ejemploDbContext(options))
            await using (var uow = new UnitOfWork(db))
            {
                await new AdministradorService(uow).ActualizarEstadoKYCAsync(adminId, usuarioId, EstadoCuenta.Suspendido);
            }
            await using (var db = new ejemploDbContext(options))
            await using (var uow = new UnitOfWork(db))
            {
                var usuario = await uow.UsuarioRepository.ReadAsync(usuarioId);
                Assert.Equal(EstadoCuenta.Suspendido, usuario!.Estado);

                var registro = Assert.Single(await uow.RegistroAuditoriaRepository.ReadAllAsync());
                Assert.Equal(adminId, registro.Administrador.Id);
                Assert.Contains("jugador", registro.Accion);

                // ReadFilter: el suspendido aparece en su estado y ya no entre los activos.
                var usuarioService = new UsuarioService(uow);
                Assert.Contains(await usuarioService.FiltrarUsuariosPorEstadoAsync(EstadoCuenta.Suspendido), u => u.Id == usuarioId);
                var activos = await usuarioService.FiltrarUsuariosPorEstadoAsync(EstadoCuenta.Activo);
                Assert.DoesNotContain(activos, u => u.Id == usuarioId);
                Assert.Contains(activos, u => u.Id == adminId && u is Administrador);

                // Con la cuenta suspendida no se puede sumar experiencia.
                await Assert.ThrowsAsync<ModelException>(() => usuarioService.SumarExperienciaAsync(usuarioId, 10));
            }

            // 5) Reactivación: dos registros de auditoría en total y la experiencia se conserva.
            await using (var db = new ejemploDbContext(options))
            await using (var uow = new UnitOfWork(db))
            {
                await new AdministradorService(uow).ActualizarEstadoKYCAsync(adminId, usuarioId, EstadoCuenta.Activo);
            }
            await using (var db = new ejemploDbContext(options))
            await using (var uow = new UnitOfWork(db))
            {
                var usuario = await uow.UsuarioRepository.ReadAsync(usuarioId);
                Assert.Equal(EstadoCuenta.Activo, usuario!.Estado);
                Assert.Equal(250, usuario.Experiencia);
                Assert.Equal(2, (await uow.RegistroAuditoriaRepository.ReadAllAsync()).Count);
                await new UsuarioService(uow).SumarExperienciaAsync(usuarioId, 50);
                Assert.Equal(4, usuario.Nivel);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}
