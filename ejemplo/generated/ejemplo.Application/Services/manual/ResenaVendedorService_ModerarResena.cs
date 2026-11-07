using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ResenaVendedor_moderarResena) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ResenaVendedorService
{
    public async Task ModerarResenaAsync(int p_oid, int p_admin_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ResenaVendedor_moderarResena) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for ResenaVendedor.moderarResena is pending.", new NotImplementedException("Method ModerarResenaAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
