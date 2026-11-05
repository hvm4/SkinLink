using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
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
        // Write here your custom code...

        throw new ModelException("Application service generation for Usuario.suspenderCuentaUsuario is pending.", new NotImplementedException("Method SuspenderCuentaUsuarioAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
