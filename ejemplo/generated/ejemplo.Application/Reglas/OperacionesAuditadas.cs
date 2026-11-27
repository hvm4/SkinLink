using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;

namespace ejemplo.Application.Reglas;

public static class OperacionesAuditadas
{
    public static Task SuspenderCuentaUsuarioAsync(IUnitOfWork unitOfWork, int administradorId, int usuarioId) =>
        EjecutarAsync(unitOfWork, administradorId, usuarioId, "SuspenderCuentaUsuario",
            service => service.SuspenderCuentaUsuarioAsync(usuarioId),
            usuario => $"Suspensión: cuenta '{usuario.NombreUsuario}' (#{usuario.Id}) suspendida");

    public static Task VerificarUsuarioAsync(IUnitOfWork unitOfWork, int administradorId, int usuarioId) =>
        EjecutarAsync(unitOfWork, administradorId, usuarioId, "VerificarUsuario",
            service => service.VerificarUsuarioAsync(usuarioId),
            usuario => $"Verificación: identidad de '{usuario.NombreUsuario}' (#{usuario.Id}) verificada");

    private static async Task EjecutarAsync(
        IUnitOfWork unitOfWork, int administradorId, int usuarioId, string nombreTransaccion,
        Func<UsuarioService, Task> operacion, Func<Usuario, string> describir)
    {
        await unitOfWork.BeginTransactionAsync(nombreTransaccion);
        try
        {
            var administrador = await unitOfWork.UsuarioRepository.ReadAsync(administradorId) as Administrador
                ?? throw new ModelException("Solo un administrador puede realizar esta operación.");
            if (administrador.Estado != ejemplo.Domain.Enums.EstadoCuenta.Activo)
                throw new ModelException("El administrador debe tener la cuenta activa.");
            if (administrador.Id == usuarioId)
                throw new ModelException("Un administrador no puede aplicar esta operación sobre su propia cuenta.");

            // La operación de negocio usa el mismo UnitOfWork: sus cambios entran en esta transacción.
            await operacion(new UsuarioService(unitOfWork));

            var usuario = await unitOfWork.UsuarioRepository.ReadAsync(usuarioId)
                ?? throw new ModelException("Usuario not found.");

            await unitOfWork.RegistroAuditoriaRepository.SaveAsync(new RegistroAuditoria
            {
                Accion = describir(usuario),
                Fecha = DateTime.UtcNow,
                Administrador = administrador
            });

            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}
