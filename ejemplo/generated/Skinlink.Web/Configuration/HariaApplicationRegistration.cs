// HARIA-WEB GENERATED sha256:e0da540db4179adf6c91f94936f5d7f25335e7f4b5353baf9fcde355972fac33
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Skinlink.Web.Configuration;

public static class HariaApplicationRegistration
{
    public static IServiceCollection AddHariaApplication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = RequireConnectionString(configuration, environment);
        services.AddDbContext<ejemplo.Infrastructure.Data.ejemploDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddScoped<ejemplo.Application.Abstractions.IHariaPasswordHasher, ejemplo.Infrastructure.Security.HariaPasswordHasher>();
        services.AddScoped<ejemplo.Application.Abstractions.IHariaTokenGenerator, ejemplo.Infrastructure.Security.HariaTokenGenerator>();
        services.AddScoped<ejemplo.Application.Abstractions.IUsuarioRepository, ejemplo.Infrastructure.Repositories.UsuarioRepository>();
        services.AddScoped<ejemplo.Application.Abstractions.IUnitOfWorkFactory, ejemplo.Infrastructure.UnitOfWorkFactory>();
        services.AddScoped<ejemplo.Application.Services.AdministradorService>();
        services.AddScoped<ejemplo.Application.Services.MonederoService>();
        services.AddScoped<ejemplo.Application.Services.TransaccionService>();
        services.AddScoped<ejemplo.Application.Services.InventarioPrivadoService>();
        services.AddScoped<ejemplo.Application.Services.ArticuloCatalogoService>();
        services.AddScoped<ejemplo.Application.Services.ActivoDigitalService>();
        services.AddScoped<ejemplo.Application.Services.FavoritoService>();
        services.AddScoped<ejemplo.Application.Services.IntercambioService>();
        services.AddScoped<ejemplo.Application.Services.VentaService>();
        services.AddScoped<ejemplo.Application.Services.ResenaVendedorService>();
        services.AddScoped<ejemplo.Application.Services.CajaMisteriosaService>();
        services.AddScoped<ejemplo.Application.Services.ProbabilidadDropService>();
        services.AddScoped<ejemplo.Application.Services.ConfiguracionSistemaService>();
        services.AddScoped<ejemplo.Application.Services.RegistroAuditoriaService>();
        services.AddScoped<ejemplo.Application.Services.UsuarioService>();
        return services;
    }

    private static string RequireConnectionString(IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DbConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure ConnectionStrings:DbConnection in appsettings.Haria.json, user-secrets, or the ConnectionStrings__DbConnection environment variable.");
        }

        var sqlite = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = connectionString };
        var dataSourceKey = sqlite.Keys.Cast<string>().FirstOrDefault(key =>
            string.Equals(key.Replace(" ", ""), "DataSource", StringComparison.OrdinalIgnoreCase)
            || string.Equals(key, "Filename", StringComparison.OrdinalIgnoreCase));
        if (dataSourceKey is null)
        {
            throw new InvalidOperationException("The SQLite connection string must specify Data Source or Filename.");
        }
        var dataSource = Convert.ToString(sqlite[dataSourceKey]) ?? string.Empty;
        if (dataSource.Length == 0 || dataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase) || Path.IsPathRooted(dataSource)
            || string.Equals(dataSource, ":memory:", StringComparison.OrdinalIgnoreCase)
            || (sqlite.TryGetValue("Mode", out var mode) && string.Equals(Convert.ToString(mode), "Memory", StringComparison.OrdinalIgnoreCase)))
        {
            return sqlite.ConnectionString;
        }

        sqlite[dataSourceKey] = Path.GetFullPath(Path.Combine(environment.ContentRootPath, dataSource));
        return sqlite.ConnectionString;
    }
}
