using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Venta_cancelarVenta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class VentaService
{
    public async Task CancelarVentaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Venta_cancelarVenta) ENABLED START*/
        // [TX] Si el código generado de CustomTransaction ya abre/cierra la transacción,
        //      elimina las líneas marcadas con [TX].
        await _unitOfWork.BeginTransactionAsync("CancelarVenta");   // [TX]
        try                                                               // [TX]
        {
            var venta = await _repository.ReadAsync(p_oid)
                ?? throw new ModelException("Venta not found.");

            // Solo se puede anular una propuesta pendiente.
            if (venta.EstadoVenta != EstadoVenta.Disponible && venta.EstadoVenta != EstadoVenta.EnProceso)
                throw new ModelException("Solo se puede cancelar una propuesta de intercambio pendiente.");

            // Carga la relación Venta -> ActivoDigital en el contexto compartido (EF hace el enlace).
            await _unitOfWork.ActivoDigitalRepository.ReadAllAsync();
            var activo = venta.ActivoDigital
                ?? throw new ModelException("La venta no tiene un activo asociado.");

            // 1) Anular la propuesta
            venta.EstadoVenta = EstadoVenta.Cancelada;
            await _repository.SaveAsync(venta);

            // 2) Liberar el activo reservado
            activo.EnVenta = false;
            await _unitOfWork.ActivoDigitalRepository.SaveAsync(activo);

            await _unitOfWork.CommitAsync();                              // [TX]
        }                                                                 // [TX]
        catch                                                             // [TX]
        {                                                                 // [TX]
            await _unitOfWork.RollbackAsync();                            // [TX]
            throw;                                                        // [TX]
        }                                                                 // [TX]
        /*PROTECTED REGION END*/
    }
}