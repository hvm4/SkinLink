// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class ProbabilidadDropAssembler
{
    /*PROTECTED REGION ID(ProbabilidadDropAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.ProbabilidadDropViewModel ToViewModel(ProbabilidadDrop entity) =>
        new()
        {
            Id = entity.Id,
            PorcentajeDrop = entity.PorcentajeDrop,
            CajaId = entity.Caja?.Id,
            CajaLabel = entity.Caja is null ? null : Convert.ToString(entity.Caja.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            ActivoRecompensaId = entity.ActivoRecompensa?.Id,
            ActivoRecompensaLabel = entity.ActivoRecompensa is null ? null : Convert.ToString(entity.ActivoRecompensa.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ProbabilidadDropAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.ProbabilidadDropViewModel model, ProbabilidadDrop entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.PorcentajeDrop = model.PorcentajeDrop;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(ProbabilidadDropAssembler_method_Create) START*/
    public static ProbabilidadDrop Create(Skinlink.Web.ViewModels.ProbabilidadDropViewModel model)
    {
        var entity = new ProbabilidadDrop();
        ApplyToEntity(model, entity);
        return entity;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(ProbabilidadDropAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
