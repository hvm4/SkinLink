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
        // Descuenta un importe del saldo disponible del monedero; nunca deja el saldo en negativo.
        // No abre transacción propia: si se llama desde un caso de uso, participa en la suya.
        if (p_montoRestar is null || double.IsNaN(p_montoRestar.Valor) || double.IsInfinity(p_montoRestar.Valor) || p_montoRestar.Valor <= 0)
            throw new ModelException("El importe a restar debe ser un número positivo.");

        var monedero = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Monedero not found.");

        var saldo = monedero.SaldoDisponible
            ?? throw new ModelException("El monedero no tiene saldo disponible.");
        if (!string.IsNullOrWhiteSpace(p_montoRestar.Divisa) && !string.Equals(saldo.Divisa, p_montoRestar.Divisa, StringComparison.OrdinalIgnoreCase))
            throw new ModelException("La divisa del importe no coincide con la del monedero.");
        if (saldo.Valor < p_montoRestar.Valor)
            throw new ModelException("Saldo insuficiente en el monedero.");

        monedero.SaldoDisponible = new Moneda
        {
            Valor = Math.Round(saldo.Valor - p_montoRestar.Valor, 2),
            Divisa = saldo.Divisa
        };
        await _repository.SaveAsync(monedero);
        /*PROTECTED REGION END*/
    }
}
