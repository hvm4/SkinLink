using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ejemplo.Application.Abstractions;
using ejemplo.Infrastructure.Utils.Manual;

namespace ejemplo.Infrastructure.Utils.Generated;

public static class ExternalUtilsServiceCollectionExtensions
{
    public static IServiceCollection AddHariaExternalUtils(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWorkUtils, UnitOfWorkUtils>();
        ExternalUtilRegistration.Register(services, configuration);
        return services;
    }
}