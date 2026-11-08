using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Usuario_verificarUsuario) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class UsuarioService
{
    public async Task VerificarUsuarioAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Usuario_verificarUsuario) ENABLED START*/
        var usuario = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException($"Usuario con id {p_oid} no encontrado.");

        if (usuario.Estado == EstadoCuenta.Suspendido)
            throw new ModelException("No se puede verificar la identidad de una cuenta suspendida.");

        if (usuario.Verificado)
            throw new ModelException("El usuario ya tiene la identidad verificada.");

        // Solo pueden operar con fondos los usuarios mayores de edad.
        DateTime hoy = DateTime.UtcNow.Date;
        int edad = hoy.Year - usuario.FechaNacimiento.Year;
        if (usuario.FechaNacimiento.Date > hoy.AddYears(-edad))
            edad--;

        if (edad < 18)
            throw new ModelException("Solo los usuarios mayores de edad pueden verificar su identidad.");

        usuario.Verificado = true;
        await _repository.SaveAsync(usuario);
        /*PROTECTED REGION END*/
    }
}
