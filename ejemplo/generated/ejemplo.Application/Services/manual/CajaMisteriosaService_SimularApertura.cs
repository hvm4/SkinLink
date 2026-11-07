using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_CajaMisteriosa_simularApertura) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class CajaMisteriosaService
{
    public async Task<ActivoDigital> SimularAperturaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_CajaMisteriosa_simularApertura) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for CajaMisteriosa.simularApertura is pending.", new NotImplementedException("Method SimularAperturaAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
