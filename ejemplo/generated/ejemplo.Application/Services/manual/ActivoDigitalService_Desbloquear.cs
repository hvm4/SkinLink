using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ActivoDigital_desbloquear) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ActivoDigitalService
{
    public async Task DesbloquearAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ActivoDigital_desbloquear) ENABLED START*/
        ActivoDigital activo = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ActivoDigital not found.");

        if (!activo.EnVenta)
            throw new ModelException("El activo no esta bloqueado.");

        // No se puede liberar un activo que tiene un anuncio de venta activo.
        if (activo.Venta is not null &&
            (activo.Venta.EstadoVenta == EstadoVenta.Disponible ||
             activo.Venta.EstadoVenta == EstadoVenta.EnProceso))
            throw new ModelException("El activo tiene una venta activa; cancela la venta antes de desbloquearlo.");

        activo.EnVenta = false;
        await _repository.SaveAsync(activo);
        /*PROTECTED REGION END*/
    }
}
