using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ResenaVendedor_moderarResena) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ResenaVendedorService
{
    public async Task ModerarResenaAsync(int p_oid, int p_admin_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ResenaVendedor_moderarResena) ENABLED START*/
        // Moderación por un Administrador (RF-26): alterna la visibilidad de la reseña
        // (visible -> oculta por moderación, oculta -> restaurada) y lo registra en auditoría (RF-29).
        // El filtro filtrarResenasVisiblesPorVendedor excluye las ocultas, así que la reputación
        // visible del vendedor se recalcula sola.
        await _unitOfWork.BeginTransactionAsync("ModerarResena");
        try
        {
            var resena = await _repository.ReadAsync(p_oid)
                ?? throw new ModelException("ResenaVendedor not found.");

            var admin = await _unitOfWork.UsuarioRepository.ReadAsync(p_admin_oid) as Administrador
                ?? throw new ModelException("Solo un administrador puede moderar reseñas.");
            if (admin.Estado != EstadoCuenta.Activo)
                throw new ModelException("El administrador debe tener la cuenta activa para moderar.");

            resena.Oculta = !resena.Oculta;
            await _repository.SaveAsync(resena);

            var vendedor = resena.Vendedor?.NombreUsuario ?? "?";
            await _unitOfWork.RegistroAuditoriaRepository.SaveAsync(new RegistroAuditoria
            {
                Accion = resena.Oculta
                    ? $"Moderación: reseña #{resena.Id} al vendedor '{vendedor}' ocultada"
                    : $"Moderación: reseña #{resena.Id} al vendedor '{vendedor}' restaurada",
                Fecha = DateTime.UtcNow,
                Administrador = admin
            });

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        /*PROTECTED REGION END*/
    }
}
