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
        // Acredita un importe en el saldo disponible del monedero.
        // No abre transacción propia: si se llama desde un caso de uso, participa en la suya.
        if (p_montoAnadir is null || double.IsNaN(p_montoAnadir.Valor) || double.IsInfinity(p_montoAnadir.Valor) || p_montoAnadir.Valor <= 0)
            throw new ModelException("El importe a añadir debe ser un número positivo.");

        var monedero = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Monedero not found.");

        var saldo = monedero.SaldoDisponible ?? new Moneda { Valor = 0, Divisa = p_montoAnadir.Divisa };
        var divisa = string.IsNullOrWhiteSpace(saldo.Divisa) ? p_montoAnadir.Divisa : saldo.Divisa;
        if (!string.IsNullOrWhiteSpace(p_montoAnadir.Divisa) && !string.Equals(divisa, p_montoAnadir.Divisa, StringComparison.OrdinalIgnoreCase))
            throw new ModelException("La divisa del importe no coincide con la del monedero.");

        monedero.SaldoDisponible = new Moneda
        {
            Valor = Math.Round(saldo.Valor + p_montoAnadir.Valor, 2),
            Divisa = divisa
        };
        await _repository.SaveAsync(monedero);
        /*PROTECTED REGION END*/
    }
}
