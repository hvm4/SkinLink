using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Administrador_actualizarEstadoKYC) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class AdministradorService
{
    public async Task ActualizarEstadoKYCAsync(int p_oid, int p_usuario_oid, EstadoCuenta p_nuevoEstado)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Administrador_actualizarEstadoKYC) ENABLED START*/
        // El Administrador revisa una cuenta y cambia su estado (activar/reactivar o suspender) (RF-24, CU-18).
        // Toda modificación queda registrada en RegistroAuditoria a nombre del administrador (RF-29).
        await _unitOfWork.BeginTransactionAsync("ActualizarEstadoKYC");
        try
        {
            // Se lee como Usuario y se comprueba el tipo: así un Id de usuario normal se rechaza sin ambigüedad.
            var admin = await _unitOfWork.UsuarioRepository.ReadAsync(p_oid) as Administrador
                ?? throw new ModelException("Solo un administrador puede cambiar el estado de una cuenta.");
            if (admin.Estado != EstadoCuenta.Activo)
                throw new ModelException("El administrador debe tener la cuenta activa para gestionar usuarios.");

            var usuario = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
                ?? throw new ModelException("Usuario not found.");

            if (usuario.Id == admin.Id)
                throw new ModelException("Un administrador no puede cambiar el estado de su propia cuenta.");
            if (usuario is Administrador)
                throw new ModelException("No se puede cambiar el estado de la cuenta de otro administrador.");
            if (!Enum.IsDefined(p_nuevoEstado))
                throw new ModelException("Estado de cuenta no válido.");
            if (usuario.Estado == p_nuevoEstado)
                throw new ModelException($"La cuenta ya está en estado {p_nuevoEstado}.");

            var estadoAnterior = usuario.Estado;
            usuario.Estado = p_nuevoEstado;
            await _unitOfWork.UsuarioRepository.SaveAsync(usuario);

            await _unitOfWork.RegistroAuditoriaRepository.SaveAsync(new RegistroAuditoria
            {
                Accion = $"KYC: cuenta '{usuario.NombreUsuario}' (#{usuario.Id}) cambia de {estadoAnterior} a {p_nuevoEstado}",
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
