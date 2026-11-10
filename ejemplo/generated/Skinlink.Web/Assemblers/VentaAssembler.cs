// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class VentaAssembler
{
    /*PROTECTED REGION ID(VentaAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.VentaViewModel ToViewModel(Venta entity) =>
        new()
        {
            Id = entity.Id,
            PrecioVentaValor = entity.PrecioVenta?.Valor ?? default,
            PrecioVentaDivisa = entity.PrecioVenta?.Divisa ?? string.Empty,
            FechaPublicacion = entity.FechaPublicacion,
            EstadoVenta = entity.EstadoVenta,
            VendedorId = entity.Vendedor?.Id,
            VendedorLabel = entity.Vendedor is null ? null : Convert.ToString(entity.Vendedor.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            CompradorId = entity.Comprador?.Id,
            CompradorLabel = entity.Comprador is null ? null : Convert.ToString(entity.Comprador.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            ActivoDigitalId = entity.ActivoDigital?.Id,
            ActivoDigitalLabel = entity.ActivoDigital is null ? null : Convert.ToString(entity.ActivoDigital.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            TransaccionCobroId = entity.TransaccionCobro?.Id,
            TransaccionCobroLabel = entity.TransaccionCobro is null ? null : Convert.ToString(entity.TransaccionCobro.Concepto, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            TransaccionPagoId = entity.TransaccionPago?.Id,
            TransaccionPagoLabel = entity.TransaccionPago is null ? null : Convert.ToString(entity.TransaccionPago.Concepto, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(VentaAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.VentaViewModel model, Venta entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.PrecioVenta ??= new global::ejemplo.Domain.ValueObjects.Moneda();
        entity.PrecioVenta!.Valor = model.PrecioVentaValor;
        entity.PrecioVenta!.Divisa = model.PrecioVentaDivisa;
        entity.FechaPublicacion = model.FechaPublicacion;
        entity.EstadoVenta = model.EstadoVenta;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(VentaAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
