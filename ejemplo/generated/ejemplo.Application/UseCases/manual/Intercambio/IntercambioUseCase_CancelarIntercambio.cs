using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_Intercambio_cancelarIntercambio) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.Intercambio;

public sealed partial class IntercambioUseCase
{
    public async Task CancelarIntercambioAsync(int p_oid, int p_usuario_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_Intercambio_cancelarIntercambio) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        IntercambioService intercambioService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("IntercambioUseCase.CancelarIntercambioAsync");
            intercambioService = new IntercambioService(_unitOfWork);

            var intercambio = await _unitOfWork.IntercambioRepository.ReadAsync(p_oid)
                ?? throw new ModelException("Intercambio not found.");

            // Carga de relaciones en el contexto compartido (EF enlaza Proponente y activos reservados)
            await _unitOfWork.UsuarioRepository.ReadAllAsync();
            var activos = await _unitOfWork.ActivoDigitalRepository.ReadAllAsync();

            if (intercambio.Estado != EstadoIntercambio.Pendiente)
                throw new ModelException("Solo se puede cancelar o rechazar un intercambio pendiente.");

            // El proponente cancela; el receptor rechaza.
            bool esProponente = intercambio.Proponente?.Id == p_usuario_oid;
            bool esReceptor = intercambio.Receptor?.Id == p_usuario_oid;
            if (!esProponente && !esReceptor)
                throw new ModelException("Solo el proponente (cancelar) o el receptor (rechazar) pueden cerrar el intercambio.");

            // Liberar todos los activos reservados por la propuesta (ofrecidos y solicitados)
            var reservados = activos
                .Where(a => a.IntercambioOfrecido?.Id == intercambio.Id
                         || a.IntercambioSolicitado?.Id == intercambio.Id)
                .ToList();

            foreach (var a in reservados)
            {
                if (a.IntercambioOfrecido?.Id == intercambio.Id)
                    a.IntercambioOfrecido = null;
                if (a.IntercambioSolicitado?.Id == intercambio.Id)
                    a.IntercambioSolicitado = null;
                await _unitOfWork.ActivoDigitalRepository.SaveAsync(a);
            }

            // Anular la propuesta
            intercambio.Estado = esProponente ? EstadoIntercambio.Cancelado : EstadoIntercambio.Rechazado;
            await _unitOfWork.IntercambioRepository.SaveAsync(intercambio);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Intercambio.cancelarIntercambio.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
