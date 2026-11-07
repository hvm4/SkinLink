using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ActivoDigital_actualizarPrecio) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ActivoDigitalService
{
    public async Task ActualizarPrecioAsync(int p_oid, Moneda p_nuevoPrecio)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ActivoDigital_actualizarPrecio) ENABLED START*/
        if (p_nuevoPrecio == null)
            throw new ModelException("El precio no puede ser nulo.");

        if (p_nuevoPrecio.Valor < 0)
            throw new ModelException("El precio no puede ser negativo.");

        var activo = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException($"ActivoDigital con id {p_oid} no encontrado.");

        activo.PrecioEstimado = new Moneda
        {
            Valor = p_nuevoPrecio.Valor,
            Divisa = p_nuevoPrecio.Divisa
        };

        await _repository.SaveAsync(activo);
        /*PROTECTED REGION END*/
    }
}
