using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Venta_publicarVenta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class VentaService
{
    public async Task PublicarVentaAsync(int p_oid, int p_activoDigital_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Venta_publicarVenta) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for Venta.publicarVenta is pending.", new NotImplementedException("Method PublicarVentaAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
