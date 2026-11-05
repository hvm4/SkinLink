using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Usuario_suspenderCuentaUsuario) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class UsuarioService
{
    public async Task SuspenderCuentaUsuarioAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Usuario_suspenderCuentaUsuario) ENABLED START*/
        var usuario = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");

        if (usuario is Administrador)
            throw new ModelException("No se puede suspender la cuenta de un administrador.");

        if (usuario.Estado == EstadoCuenta.Suspendido)
            throw new ModelException("La cuenta ya está suspendida.");

        usuario.Estado = EstadoCuenta.Suspendido;
        await _repository.SaveAsync(usuario);
        /*PROTECTED REGION END*/
    }
}