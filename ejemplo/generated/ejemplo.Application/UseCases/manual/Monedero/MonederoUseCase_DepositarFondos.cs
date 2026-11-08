using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_Monedero_depositarFondos) ENABLED START*/
        // references to other libraries
        using ejemplo.Domain.ValueObjects;
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.Monedero;

public sealed partial class MonederoUseCase
{
    public async Task<global::ejemplo.Domain.Entities.Transaccion> DepositarFondosAsync(int p_oid, global::ejemplo.Domain.ValueObjects.Moneda p_monto, string p_referenciaPago, bool p_pagoConfirmado)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_Monedero_depositarFondos) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        MonederoService monederoService = null!;
        global::ejemplo.Domain.Entities.Transaccion result = default(global::ejemplo.Domain.Entities.Transaccion)!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("MonederoUseCase.DepositarFondosAsync");
            monederoService = new MonederoService(_unitOfWork);

            if (!p_pagoConfirmado)
                throw new ModelException("La pasarela de pago no ha confirmado el pago: no se ingresan fondos.");

            if (string.IsNullOrWhiteSpace(p_referenciaPago))
                throw new ModelException("Se necesita la referencia de pago de la pasarela.");

            if (p_monto == null || double.IsNaN(p_monto.Valor) || p_monto.Valor <= 0)
                throw new ModelException("El importe a depositar debe ser mayor que cero.");

            var monedero = await _unitOfWork.MonederoRepository.ReadAsync(p_oid)
                ?? throw new ModelException($"Monedero con id {p_oid} no encontrado.");

            // Cargar relaciones en el contexto compartido (EF las enlaza)
            await _unitOfWork.UsuarioRepository.ReadAllAsync();
            await _unitOfWork.MonederoRepository.ReadAllAsync();
            var transacciones = await _unitOfWork.TransaccionRepository.ReadAllAsync();

            var usuario = monedero.Usuario
                ?? throw new ModelException("El monedero no tiene un usuario asociado.");

            if (usuario.Estado != EstadoCuenta.Activo)
                throw new ModelException("La cuenta del usuario no está activa.");

            if (!string.Equals(monedero.SaldoDisponible.Divisa, p_monto.Divisa, StringComparison.OrdinalIgnoreCase))
                throw new ModelException("La divisa del depósito no coincide con la del monedero.");

            // La misma referencia de la pasarela no puede acreditarse dos veces.
            string concepto = $"Depósito confirmado en pasarela. Ref: {p_referenciaPago.Trim()}";
            if (transacciones.Any(t => t.Tipo == TipoTransaccion.Deposito
                                       && string.Equals(t.Concepto, concepto, StringComparison.OrdinalIgnoreCase)))
                throw new ModelException("Esa referencia de pago ya fue acreditada.");

            // 1) Ingresar el importe en el monedero
            monedero.SaldoDisponible = new Moneda
            {
                Valor = monedero.SaldoDisponible.Valor + p_monto.Valor,
                Divisa = monedero.SaldoDisponible.Divisa
            };
            await _unitOfWork.MonederoRepository.SaveAsync(monedero);

            // 2) Registrar la transacción de depósito
            result = new global::ejemplo.Domain.Entities.Transaccion
            {
                Tipo = TipoTransaccion.Deposito,
                Monto = new Moneda { Valor = p_monto.Valor, Divisa = p_monto.Divisa },
                Fecha = DateTime.UtcNow,
                Concepto = concepto,
                Monedero = monedero
            };
            await _unitOfWork.TransaccionRepository.SaveAsync(result);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Monedero.depositarFondos.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
