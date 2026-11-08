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
        // Write here your custom code...

        throw new ModelException("Application service generation for Administrador.actualizarEstadoKYC is pending.", new NotImplementedException("Method ActualizarEstadoKYCAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
