using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ActivoDigital_bloquearParaVenta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ActivoDigitalService
{
    public async Task BloquearParaVentaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ActivoDigital_bloquearParaVenta) ENABLED START*/
        ActivoDigital activo = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ActivoDigital not found.");

        if (activo.EnVenta)
            throw new ModelException("El activo ya esta bloqueado para la venta.");

        activo.EnVenta = true;
        await _repository.SaveAsync(activo);
        /*PROTECTED REGION END*/
    }
}
