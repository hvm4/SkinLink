using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Venta_editarPrecio) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class VentaService
{
    public async Task EditarPrecioAsync(int p_oid, Moneda p_nuevoPrecio)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Venta_editarPrecio) ENABLED START*/
        // Cambia el precio de un anuncio mientras no haya sido comprado (RF-17, CU-14).
        if (p_nuevoPrecio is null || double.IsNaN(p_nuevoPrecio.Valor) || double.IsInfinity(p_nuevoPrecio.Valor) || p_nuevoPrecio.Valor <= 0)
            throw new ModelException("El nuevo precio debe ser un número positivo.");

        var venta = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Venta not found.");

        if (venta.EstadoVenta != EstadoVenta.Disponible)
            throw new ModelException("Solo se puede editar el precio de una venta disponible.");

        var divisaActual = venta.PrecioVenta?.Divisa ?? string.Empty;
        var divisa = string.IsNullOrWhiteSpace(p_nuevoPrecio.Divisa) ? divisaActual : p_nuevoPrecio.Divisa;
        if (string.IsNullOrWhiteSpace(divisa))
            throw new ModelException("El precio debe indicar la divisa.");
        if (!string.IsNullOrWhiteSpace(divisaActual) && !string.Equals(divisa, divisaActual, StringComparison.OrdinalIgnoreCase))
            throw new ModelException("No se puede cambiar la divisa de un anuncio publicado.");

        venta.PrecioVenta = new Moneda { Valor = Math.Round(p_nuevoPrecio.Valor, 2), Divisa = divisaActual.Length > 0 ? divisaActual : divisa };
        await _repository.SaveAsync(venta);
        /*PROTECTED REGION END*/
    }
}
