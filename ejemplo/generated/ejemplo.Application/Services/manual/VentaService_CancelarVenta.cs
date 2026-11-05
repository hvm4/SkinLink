using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Venta_cancelarVenta) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class VentaService
{
    public async Task CancelarVentaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Venta_cancelarVenta) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for Venta.cancelarVenta is pending.", new NotImplementedException("Method CancelarVentaAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
