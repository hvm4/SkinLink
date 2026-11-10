// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class CajaMisteriosaAssembler
{
    /*PROTECTED REGION ID(CajaMisteriosaAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.CajaMisteriosaViewModel ToViewModel(CajaMisteriosa entity) =>
        new()
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            PrecioAperturaValor = entity.PrecioApertura?.Valor ?? default,
            PrecioAperturaDivisa = entity.PrecioApertura?.Divisa ?? string.Empty,
            Activa = entity.Activa,
            Probabilidades = entity.Probabilidades.Select(ProbabilidadDropAssembler.ToViewModel).ToList(),
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CajaMisteriosaAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.CajaMisteriosaViewModel model, CajaMisteriosa entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Nombre = model.Nombre;
        entity.PrecioApertura ??= new global::ejemplo.Domain.ValueObjects.Moneda();
        entity.PrecioApertura!.Valor = model.PrecioAperturaValor;
        entity.PrecioApertura!.Divisa = model.PrecioAperturaDivisa;
        entity.Activa = model.Activa;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CajaMisteriosaAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
