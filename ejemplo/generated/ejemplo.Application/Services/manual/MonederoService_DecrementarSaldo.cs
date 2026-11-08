using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Monedero_decrementarSaldo) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class MonederoService
{
    public async Task DecrementarSaldoAsync(int p_oid, Moneda p_montoRestar)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Monedero_decrementarSaldo) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for Monedero.decrementarSaldo is pending.", new NotImplementedException("Method DecrementarSaldoAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
