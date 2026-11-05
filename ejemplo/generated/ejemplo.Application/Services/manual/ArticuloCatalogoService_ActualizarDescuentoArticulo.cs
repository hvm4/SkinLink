using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ArticuloCatalogo_actualizarDescuentoArticulo) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ArticuloCatalogoService
{
    public async Task ActualizarDescuentoArticuloAsync(int p_oid, double p_porcentaje, DateTime p_fechaFin)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ArticuloCatalogo_actualizarDescuentoArticulo) ENABLED START*/
        if (double.IsNaN(p_porcentaje) || p_porcentaje < 0 || p_porcentaje > 100)
            throw new ModelException("El porcentaje de descuento debe estar entre 0 y 100.");

        // Descuento temporal: debe terminar en el futuro.
        if (p_fechaFin <= DateTime.UtcNow)
            throw new ModelException("La fecha de fin del descuento debe ser posterior a la fecha actual.");

        var articulo = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ArticuloCatalogo not found.");

        articulo.PorcentajeDescuento = p_porcentaje;
        articulo.FechaFinDescuento = p_fechaFin;

        await _repository.SaveAsync(articulo);
        /*PROTECTED REGION END*/
    }
}
