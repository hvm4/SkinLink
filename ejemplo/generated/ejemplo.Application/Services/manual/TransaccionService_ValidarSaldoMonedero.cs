using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Transaccion_validarSaldoMonedero) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class TransaccionService
{
    public async Task<bool> ValidarSaldoMonederoAsync(int p_oid, int p_monedero_oid, Moneda p_montoRequerido)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Transaccion_validarSaldoMonedero) ENABLED START*/
        // Write here your custom code...

        throw new ModelException("Application service generation for Transaccion.validarSaldoMonedero is pending.", new NotImplementedException("Method ValidarSaldoMonederoAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
