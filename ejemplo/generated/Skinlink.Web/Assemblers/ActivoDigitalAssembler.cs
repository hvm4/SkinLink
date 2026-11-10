// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class ActivoDigitalAssembler
{
    /*PROTECTED REGION ID(ActivoDigitalAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.ActivoDigitalViewModel ToViewModel(ActivoDigital entity) =>
        new()
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Coleccion = entity.Coleccion,
            PrecioEstimadoValor = entity.PrecioEstimado?.Valor ?? default,
            PrecioEstimadoDivisa = entity.PrecioEstimado?.Divisa ?? string.Empty,
            AtributosRareza = entity.Atributos?.Rareza ?? default,
            AtributosCondicion = entity.Atributos?.Condicion ?? default,
            AtributosFloatVal = entity.Atributos?.FloatVal ?? default,
            AtributosPatron = entity.Atributos?.Patron ?? default,
            EnVenta = entity.EnVenta,
            IdentificadorUnico = entity.IdentificadorUnico,
            InventarioId = entity.Inventario?.Id,
            InventarioLabel = entity.Inventario is null ? null : Convert.ToString(entity.Inventario.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            VentaId = entity.Venta?.Id,
            VentaLabel = entity.Venta is null ? null : Convert.ToString(entity.Venta.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            IntercambioOfrecidoId = entity.IntercambioOfrecido?.Id,
            IntercambioOfrecidoLabel = entity.IntercambioOfrecido is null ? null : Convert.ToString(entity.IntercambioOfrecido.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            IntercambioSolicitadoId = entity.IntercambioSolicitado?.Id,
            IntercambioSolicitadoLabel = entity.IntercambioSolicitado is null ? null : Convert.ToString(entity.IntercambioSolicitado.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ActivoDigitalAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.ActivoDigitalViewModel model, ActivoDigital entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Nombre = model.Nombre;
        entity.Coleccion = model.Coleccion;
        entity.PrecioEstimado ??= new global::ejemplo.Domain.ValueObjects.Moneda();
        entity.PrecioEstimado!.Valor = model.PrecioEstimadoValor;
        entity.PrecioEstimado!.Divisa = model.PrecioEstimadoDivisa;
        entity.Atributos ??= new global::ejemplo.Domain.ValueObjects.AtributosSkin();
        entity.Atributos!.Rareza = model.AtributosRareza;
        entity.Atributos!.Condicion = model.AtributosCondicion;
        entity.Atributos!.FloatVal = model.AtributosFloatVal;
        entity.Atributos!.Patron = model.AtributosPatron;
        entity.EnVenta = model.EnVenta;
        entity.IdentificadorUnico = model.IdentificadorUnico;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(ActivoDigitalAssembler_method_Create) START*/
    public static ActivoDigital Create(Skinlink.Web.ViewModels.ActivoDigitalViewModel model)
    {
        var entity = new ActivoDigital();
        ApplyToEntity(model, entity);
        return entity;
    }
    /*PROTECTED REGION END*/
    /*PROTECTED REGION ID(ActivoDigitalAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
