using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Venta_ejecutarCompra) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class VentaService
{
    public async Task EjecutarCompraAsync(int p_oid, int p_comprador_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Venta_ejecutarCompra) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for Venta.ejecutarCompra is pending.", new NotImplementedException("Method EjecutarCompraAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
