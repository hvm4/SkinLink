using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Monedero_incrementarSaldo) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class MonederoService
{
    public async Task IncrementarSaldoAsync(int p_oid, Moneda p_montoAnadir)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Monedero_incrementarSaldo) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for Monedero.incrementarSaldo is pending.", new NotImplementedException("Method IncrementarSaldoAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
