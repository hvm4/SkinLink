using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Venta_editarPrecio) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class VentaService
{
    public async Task EditarPrecioAsync(int p_oid, Moneda p_nuevoPrecio)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Venta_editarPrecio) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for Venta.editarPrecio is pending.", new NotImplementedException("Method EditarPrecioAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
