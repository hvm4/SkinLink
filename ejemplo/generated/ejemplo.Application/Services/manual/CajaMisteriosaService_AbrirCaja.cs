using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_CajaMisteriosa_abrirCaja) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class CajaMisteriosaService
{
    public async Task AbrirCajaAsync(int p_oid, int p_usuario_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_CajaMisteriosa_abrirCaja) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for CajaMisteriosa.abrirCaja is pending.", new NotImplementedException("Method AbrirCajaAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
