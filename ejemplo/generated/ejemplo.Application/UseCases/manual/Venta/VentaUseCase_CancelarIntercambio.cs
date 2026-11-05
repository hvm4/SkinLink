using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_Venta_cancelarIntercambio) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.Venta;

public sealed partial class VentaUseCase
{
    public async Task CancelarIntercambioAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_Venta_cancelarIntercambio) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        VentaService ventaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("VentaUseCase.CancelarIntercambioAsync");
            ventaService = new VentaService(_unitOfWork);

            // Write here your custom transaction code...
            // Use ventaService, _unitOfWork.VentaRepository or _utils!.GetRequiredService<TService>() as the generated CP equivalent.

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Venta.cancelarIntercambio.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
