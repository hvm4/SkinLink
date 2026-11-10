// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class FavoritoAssembler
{
    /*PROTECTED REGION ID(FavoritoAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.FavoritoViewModel ToViewModel(Favorito entity) =>
        new()
        {
            Id = entity.Id,
            FechaMarcado = entity.FechaMarcado,
            UsuarioId = entity.Usuario?.Id,
            UsuarioLabel = entity.Usuario is null ? null : Convert.ToString(entity.Usuario.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            ActivoDigitalId = entity.ActivoDigital?.Id,
            ActivoDigitalLabel = entity.ActivoDigital is null ? null : Convert.ToString(entity.ActivoDigital.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(FavoritoAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.FavoritoViewModel model, Favorito entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.FechaMarcado = model.FechaMarcado;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(FavoritoAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
