// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class ConfiguracionSistemaAssembler
{
    /*PROTECTED REGION ID(ConfiguracionSistemaAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.ConfiguracionSistemaViewModel ToViewModel(ConfiguracionSistema entity) =>
        new()
        {
            Id = entity.Id,
            LimitePrecioBajo = entity.LimitePrecioBajo,
            LimitePrecioMedio = entity.LimitePrecioMedio,
            ComisionPrecioBajo = entity.ComisionPrecioBajo,
            ComisionPrecioMedio = entity.ComisionPrecioMedio,
            ComisionPrecioAlto = entity.ComisionPrecioAlto,
            RetiroMinimo = entity.RetiroMinimo,
            RetiroMaximoDiario = entity.RetiroMaximoDiario,

        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ConfiguracionSistemaAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.ConfiguracionSistemaViewModel model, ConfiguracionSistema entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.LimitePrecioBajo = model.LimitePrecioBajo;
        entity.LimitePrecioMedio = model.LimitePrecioMedio;
        entity.ComisionPrecioBajo = model.ComisionPrecioBajo;
        entity.ComisionPrecioMedio = model.ComisionPrecioMedio;
        entity.ComisionPrecioAlto = model.ComisionPrecioAlto;
        entity.RetiroMinimo = model.RetiroMinimo;
        entity.RetiroMaximoDiario = model.RetiroMaximoDiario;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ConfiguracionSistemaAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
