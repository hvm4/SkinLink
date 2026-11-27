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

public sealed class UsuariosYAdministracionTests
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

    private static Monedero NuevoMonedero() => new Monedero
    {
        SaldoDisponible = new Moneda { Valor = 0, Divisa = "EUR" },
        SaldoPendiente = new Moneda { Valor = 0, Divisa = "EUR" }
    };

    private static InventarioPrivado NuevoInventario() =>
        new InventarioPrivado { ValorEstimadoTotal = new Moneda { Valor = 0, Divisa = "EUR" }, EsPublico = false };

    private static Usuario NuevoUsuario(string nombre, EstadoCuenta estado = EstadoCuenta.Activo, int experiencia = 0, int nivel = 1, bool verificado = false) => new Usuario
    {
        NombreUsuario = nombre,
        Correo = nombre + "@test.com",
        Contrasena = "pwd123",
        FechaNacimiento = DateTime.UtcNow.AddYears(-25),
        Estado = estado,
        Experiencia = experiencia,
        Nivel = nivel,
        Verificado = verificado,
        Idioma = "es",
        Monedero = NuevoMonedero(),
        Inventario = NuevoInventario()
    };

    private static Administrador NuevoAdministrador(string nombre, EstadoCuenta estado = EstadoCuenta.Activo) => new Administrador
    {
        NombreUsuario = nombre,
        Correo = nombre + "@skinlink.com",
        Contrasena = "admin123",
        FechaNacimiento = DateTime.UtcNow.AddYears(-30),
        Estado = estado,
        Nivel = 1,
        Verificado = true,
        Idioma = "es",
        CodigoAuditor = "AUD-" + nombre,
        Monedero = NuevoMonedero(),
        Inventario = NuevoInventario()
    };

    // ---------------- sumarExperiencia (Custom) ----------------

    [Fact]
    public async Task Usuario_SumarExperiencia_No_Debe_Bajar_El_Nivel_Asignado()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);
            var usuario = NuevoUsuario("Veterano", experiencia: 0, nivel: 7);
            await uow.UsuarioRepository.SaveAsync(usuario);

            await new UsuarioService(uow).SumarExperienciaAsync(usuario.Id, 10);

            Assert.Equal(10, usuario.Experiencia);
            Assert.Equal(7, usuario.Nivel);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Usuario_SumarExperiencia_Sobre_Un_Administrador_Funciona_Por_Herencia()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);
            var admin = NuevoAdministrador("AdminXP");
            await uow.AdministradorRepository.SaveAsync(admin);

            await new UsuarioService(uow).SumarExperienciaAsync(admin.Id, 150);

            Assert.Equal(150, admin.Experiencia);
            Assert.Equal(2, admin.Nivel);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Usuario_SumarExperiencia_Debe_Rechazar_Puntos_No_Positivos_Sin_Cambiar_Nada(int puntos)
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);
            var usuario = NuevoUsuario("Rango", experiencia: 40);
            await uow.UsuarioRepository.SaveAsync(usuario);

            await Assert.ThrowsAsync<ModelException>(() => new UsuarioService(uow).SumarExperienciaAsync(usuario.Id, puntos));
            Assert.Equal(40, usuario.Experiencia);
            Assert.Equal(1, usuario.Nivel);
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ---------------- actualizarEstadoKYC (Custom de Administrador) ----------------

    [Fact]
    public async Task Administrador_ActualizarEstadoKYC_Debe_Reactivar_Una_Cuenta_Suspendida_Y_Auditar()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);
            var admin = NuevoAdministrador("Admin");
            var usuario = NuevoUsuario("Castigado", EstadoCuenta.Suspendido);
            await uow.AdministradorRepository.SaveAsync(admin);
            await uow.UsuarioRepository.SaveAsync(usuario);

            await new AdministradorService(uow).ActualizarEstadoKYCAsync(admin.Id, usuario.Id, EstadoCuenta.Activo);

            Assert.Equal(EstadoCuenta.Activo, usuario.Estado);
            var registro = Assert.Single(await uow.RegistroAuditoriaRepository.ReadAllAsync());
            Assert.Equal(admin.Id, registro.Administrador.Id);
            Assert.Contains("Castigado", registro.Accion);
            Assert.Contains("Activo", registro.Accion);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Administrador_ActualizarEstadoKYC_Debe_Rechazar_Casos_Invalidos_Sin_Dejar_Auditoria()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);
            var admin = NuevoAdministrador("Admin");
            var otroAdmin = NuevoAdministrador("OtroAdmin");
            var adminSuspendido = NuevoAdministrador("AdminSuspendido", EstadoCuenta.Suspendido);
            var usuario = NuevoUsuario("Cliente");
            await uow.AdministradorRepository.SaveAsync(admin);
            await uow.AdministradorRepository.SaveAsync(otroAdmin);
            await uow.AdministradorRepository.SaveAsync(adminSuspendido);
            await uow.UsuarioRepository.SaveAsync(usuario);

            var service = new AdministradorService(uow);

            // Mismo estado que ya tiene
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(admin.Id, usuario.Id, EstadoCuenta.Activo));
            // Sobre sí mismo
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(admin.Id, admin.Id, EstadoCuenta.Suspendido));
            // Sobre otro administrador
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(admin.Id, otroAdmin.Id, EstadoCuenta.Suspendido));
            // Administrador suspendido
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(adminSuspendido.Id, usuario.Id, EstadoCuenta.Suspendido));
            // Usuario inexistente
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(admin.Id, 99999, EstadoCuenta.Suspendido));
            // Estado fuera del enumerado
            await Assert.ThrowsAsync<ModelException>(() => service.ActualizarEstadoKYCAsync(admin.Id, usuario.Id, (EstadoCuenta)99));

            Assert.Equal(EstadoCuenta.Activo, usuario.Estado);
            Assert.Equal(EstadoCuenta.Activo, otroAdmin.Estado);
            Assert.Empty(await uow.RegistroAuditoriaRepository.ReadAllAsync());
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Administrador_ActualizarEstadoKYC_No_Puede_Ejecutarlo_Un_Usuario_Normal()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);
            var normal = NuevoUsuario("Normal");
            var victima = NuevoUsuario("Victima");
            await uow.UsuarioRepository.SaveAsync(normal);
            await uow.UsuarioRepository.SaveAsync(victima);

            await Assert.ThrowsAsync<ModelException>(() =>
                new AdministradorService(uow).ActualizarEstadoKYCAsync(normal.Id, victima.Id, EstadoCuenta.Suspendido));
            Assert.Equal(EstadoCuenta.Activo, victima.Estado);
            Assert.Empty(await uow.RegistroAuditoriaRepository.ReadAllAsync());
        }
        finally
        {
            connection.Dispose();
        }
    }

    // ---------------- filtrarUsuariosPorEstado (ReadFilter) y herencia ----------------

    [Fact]
    public async Task Usuario_FiltrarUsuariosPorEstado_Debe_Incluir_Administradores_Por_Herencia()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            await using var db = new ejemploDbContext(options);
            await using var uow = new UnitOfWork(db);
            var activo = NuevoUsuario("Activo");
            var suspendido = NuevoUsuario("Suspendido", EstadoCuenta.Suspendido);
            var admin = NuevoAdministrador("Admin");
            await uow.UsuarioRepository.SaveAsync(activo);
            await uow.UsuarioRepository.SaveAsync(suspendido);
            await uow.AdministradorRepository.SaveAsync(admin);

            var service = new UsuarioService(uow);
            var activos = await service.FiltrarUsuariosPorEstadoAsync(EstadoCuenta.Activo);
            var suspendidos = await service.FiltrarUsuariosPorEstadoAsync(EstadoCuenta.Suspendido);

            Assert.Equal(2, activos.Count);
            Assert.Contains(activos, u => u.Id == activo.Id);
            Assert.Contains(activos, u => u is Administrador a && a.CodigoAuditor == "AUD-Admin");
            Assert.Equal(suspendido.Id, Assert.Single(suspendidos).Id);
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Fact]
    public async Task Herencia_Leer_Un_Administrador_Como_Usuario_Devuelve_La_Subclase()
    {
        var (connection, options) = CreateInMemorySqlite();
        try
        {
            int adminId;
            await using (var db = new ejemploDbContext(options))
            await using (var uow = new UnitOfWork(db))
            {
                var admin = NuevoAdministrador("Jefe");
                await uow.AdministradorRepository.SaveAsync(admin);
                adminId = admin.Id;
            }

            // Contexto nuevo: nada en caché, EF tiene que resolver la subclase desde la BD.
            await using var db2 = new ejemploDbContext(options);
            await using var uow2 = new UnitOfWork(db2);
            var leido = await uow2.UsuarioRepository.ReadAsync(adminId);

            var admin2 = Assert.IsType<Administrador>(leido);
            Assert.Equal("AUD-Jefe", admin2.CodigoAuditor);
            Assert.NotNull(admin2.Monedero);
            Assert.NotNull(admin2.Inventario);
            Assert.Contains(await new UsuarioService(uow2).ReadAllAsync(), u => u.Id == adminId);
        }
        finally
        {
            connection.Dispose();
        }
    }
}
