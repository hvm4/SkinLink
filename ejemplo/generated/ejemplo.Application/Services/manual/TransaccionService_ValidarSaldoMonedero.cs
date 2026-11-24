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
        // Indica si el monedero tiene saldo disponible suficiente (y en la misma divisa) para cubrir el importe.
        // p_oid (Transaccion) no interviene en la comprobación: la operación está declarada en Transaccion
        // porque se usa antes de registrar un movimiento; puede pasarse 0.
        if (p_montoRequerido is null || double.IsNaN(p_montoRequerido.Valor) || double.IsInfinity(p_montoRequerido.Valor) || p_montoRequerido.Valor < 0)
            throw new ModelException("El importe requerido no es válido.");

        var monedero = await _unitOfWork.MonederoRepository.ReadAsync(p_monedero_oid)
            ?? throw new ModelException("Monedero not found.");

        var saldo = monedero.SaldoDisponible;
        if (saldo is null) return false;
        if (!string.IsNullOrWhiteSpace(p_montoRequerido.Divisa) && !string.Equals(saldo.Divisa, p_montoRequerido.Divisa, StringComparison.OrdinalIgnoreCase))
            return false;

        return saldo.Valor >= p_montoRequerido.Valor;
        /*PROTECTED REGION END*/
    }
}
