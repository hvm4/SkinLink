using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_CajaMisteriosa_abrirCajaSorpresa) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.CajaMisteriosa;

public sealed partial class CajaMisteriosaUseCase
{
    public async Task<global::ejemplo.Domain.Entities.ActivoDigital> AbrirCajaSorpresaAsync(int p_oid, int p_usuario_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_CajaMisteriosa_abrirCajaSorpresa) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        CajaMisteriosaService cajaMisteriosaService = null!;
        global::ejemplo.Domain.Entities.ActivoDigital result = default(global::ejemplo.Domain.Entities.ActivoDigital)!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("CajaMisteriosaUseCase.AbrirCajaSorpresaAsync");
            cajaMisteriosaService = new CajaMisteriosaService(_unitOfWork);

            // Write here your custom transaction code...
            // Use cajaMisteriosaService, _unitOfWork.CajaMisteriosaRepository or _utils!.GetRequiredService<TService>() as the generated CP equivalent.

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case CajaMisteriosa.abrirCajaSorpresa.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
