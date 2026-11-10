// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class ArticuloCatalogoAssembler
{
    /*PROTECTED REGION ID(ArticuloCatalogoAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.ArticuloCatalogoViewModel ToViewModel(ArticuloCatalogo entity) =>
        new()
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Coleccion = entity.Coleccion,
            Rareza = entity.Rareza,
            PrecioReferenciaValor = entity.PrecioReferencia?.Valor ?? default,
            PrecioReferenciaDivisa = entity.PrecioReferencia?.Divisa ?? string.Empty,
            Habilitado = entity.Habilitado,
            PorcentajeDescuento = entity.PorcentajeDescuento,
            FechaFinDescuento = entity.FechaFinDescuento,

        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ArticuloCatalogoAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.ArticuloCatalogoViewModel model, ArticuloCatalogo entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Nombre = model.Nombre;
        entity.Coleccion = model.Coleccion;
        entity.Rareza = model.Rareza;
        entity.PrecioReferencia ??= new global::ejemplo.Domain.ValueObjects.Moneda();
        entity.PrecioReferencia!.Valor = model.PrecioReferenciaValor;
        entity.PrecioReferencia!.Divisa = model.PrecioReferenciaDivisa;
        entity.Habilitado = model.Habilitado;
        entity.PorcentajeDescuento = model.PorcentajeDescuento;
        entity.FechaFinDescuento = model.FechaFinDescuento;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ArticuloCatalogoAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
