// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class TransaccionAssembler
{
    /*PROTECTED REGION ID(TransaccionAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.TransaccionViewModel ToViewModel(Transaccion entity) =>
        new()
        {
            Id = entity.Id,
            Tipo = entity.Tipo,
            MontoValor = entity.Monto?.Valor ?? default,
            MontoDivisa = entity.Monto?.Divisa ?? string.Empty,
            Fecha = entity.Fecha,
            Concepto = entity.Concepto,
            MonederoId = entity.Monedero?.Id,
            MonederoLabel = entity.Monedero is null ? null : Convert.ToString(entity.Monedero.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            VentaCobroId = entity.VentaCobro?.Id,
            VentaCobroLabel = entity.VentaCobro is null ? null : Convert.ToString(entity.VentaCobro.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            VentaPagoId = entity.VentaPago?.Id,
            VentaPagoLabel = entity.VentaPago is null ? null : Convert.ToString(entity.VentaPago.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(TransaccionAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.TransaccionViewModel model, Transaccion entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Tipo = model.Tipo;
        entity.Monto ??= new global::ejemplo.Domain.ValueObjects.Moneda();
        entity.Monto!.Valor = model.MontoValor;
        entity.Monto!.Divisa = model.MontoDivisa;
        entity.Fecha = model.Fecha;
        entity.Concepto = model.Concepto;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(TransaccionAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
