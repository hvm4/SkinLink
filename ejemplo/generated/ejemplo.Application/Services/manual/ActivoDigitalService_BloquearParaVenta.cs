using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ActivoDigital_bloquearParaVenta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ActivoDigitalService
{
    public async Task BloquearParaVentaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ActivoDigital_bloquearParaVenta) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for ActivoDigital.bloquearParaVenta is pending.", new NotImplementedException("Method BloquearParaVentaAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
