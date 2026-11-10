// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class IntercambioAssembler
{
    /*PROTECTED REGION ID(IntercambioAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.IntercambioViewModel ToViewModel(Intercambio entity) =>
        new()
        {
            Id = entity.Id,
            Estado = entity.Estado,
            FechaCreacion = entity.FechaCreacion,
            ProponenteId = entity.Proponente?.Id,
            ProponenteLabel = entity.Proponente is null ? null : Convert.ToString(entity.Proponente.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            ReceptorId = entity.Receptor?.Id,
            ReceptorLabel = entity.Receptor is null ? null : Convert.ToString(entity.Receptor.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(IntercambioAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.IntercambioViewModel model, Intercambio entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Estado = model.Estado;
        entity.FechaCreacion = model.FechaCreacion;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(IntercambioAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
