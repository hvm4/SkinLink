using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ArticuloCatalogo_actualizarDescuentoArticulo) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ArticuloCatalogoService
{
    public async Task ActualizarDescuentoArticuloAsync(int p_oid, double p_porcentaje, DateTime p_fechaFin)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ArticuloCatalogo_actualizarDescuentoArticulo) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for ArticuloCatalogo.actualizarDescuentoArticulo is pending.", new NotImplementedException("Method ActualizarDescuentoArticuloAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
