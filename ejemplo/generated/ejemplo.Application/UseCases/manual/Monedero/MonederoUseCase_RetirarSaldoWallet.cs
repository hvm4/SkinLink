using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_Monedero_retirarSaldoWallet) ENABLED START*/
        // references to other libraries
        using ejemplo.Domain.ValueObjects;
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.Monedero;

public sealed partial class MonederoUseCase
{
    public async Task<global::ejemplo.Domain.Entities.Transaccion> RetirarSaldoWalletAsync(int p_oid, global::ejemplo.Domain.ValueObjects.Moneda p_monto, string p_destino)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_Monedero_retirarSaldoWallet) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        MonederoService monederoService = null!;
        global::ejemplo.Domain.Entities.Transaccion result = default(global::ejemplo.Domain.Entities.Transaccion)!;

        // Límites por defecto si todavía no existe ninguna ConfiguracionSistema.
        const double RetiroMinimoPorDefecto = 1d;
        const double RetiroMaximoDiarioPorDefecto = 1000d;

        try
        {
            await _unitOfWork.BeginTransactionAsync("MonederoUseCase.RetirarSaldoWalletAsync");
            monederoService = new MonederoService(_unitOfWork);

            if (string.IsNullOrWhiteSpace(p_destino))
                throw new ModelException("Se necesita el destino de la retirada (pasarela o criptocartera).");

            if (p_monto == null || double.IsNaN(p_monto.Valor) || p_monto.Valor <= 0)
                throw new ModelException("El importe a retirar debe ser mayor que cero.");

            var monedero = await _unitOfWork.MonederoRepository.ReadAsync(p_oid)
                ?? throw new ModelException($"Monedero con id {p_oid} no encontrado.");

            // Cargar relaciones en el contexto compartido (EF las enlaza)
            await _unitOfWork.UsuarioRepository.ReadAllAsync();
            await _unitOfWork.MonederoRepository.ReadAllAsync();
            var transacciones = await _unitOfWork.TransaccionRepository.ReadAllAsync();
            var configuraciones = await _unitOfWork.ConfiguracionSistemaRepository.ReadAllAsync();

            var usuario = monedero.Usuario
                ?? throw new ModelException("El monedero no tiene un usuario asociado.");

            if (usuario.Estado != EstadoCuenta.Activo)
                throw new ModelException("La cuenta del usuario no está activa.");

            if (!usuario.Verificado)
                throw new ModelException("El usuario debe tener la identidad verificada para retirar saldo.");

            if (!string.Equals(monedero.SaldoDisponible.Divisa, p_monto.Divisa, StringComparison.OrdinalIgnoreCase))
                throw new ModelException("La divisa de la retirada no coincide con la del monedero.");

            var configuracion = configuraciones.FirstOrDefault();
            double minimo = configuracion?.RetiroMinimo ?? RetiroMinimoPorDefecto;
            double maximoDiario = configuracion?.RetiroMaximoDiario ?? RetiroMaximoDiarioPorDefecto;

            if (p_monto.Valor < minimo)
                throw new ModelException($"El importe mínimo de retirada es {minimo}.");

            if (monedero.SaldoDisponible.Valor < p_monto.Valor)
                throw new ModelException("Saldo insuficiente en el monedero.");

            // Límite diario: suma de las retiradas de hoy más la solicitada.
            DateTime hoy = DateTime.UtcNow.Date;
            double retiradoHoy = transacciones
                .Where(t => t.Tipo == TipoTransaccion.Retiro
                            && t.Monedero?.Id == monedero.Id
                            && t.Fecha.Date == hoy)
                .Sum(t => t.Monto.Valor);

            if (retiradoHoy + p_monto.Valor > maximoDiario)
                throw new ModelException($"Se supera el límite diario de retirada ({maximoDiario}). Ya retirado hoy: {retiradoHoy}.");

            // 1) Descontar el importe del monedero
            monedero.SaldoDisponible = new Moneda
            {
                Valor = monedero.SaldoDisponible.Valor - p_monto.Valor,
                Divisa = monedero.SaldoDisponible.Divisa
            };
            await _unitOfWork.MonederoRepository.SaveAsync(monedero);

            // 2) Registrar la transacción de retirada
            result = new global::ejemplo.Domain.Entities.Transaccion
            {
                Tipo = TipoTransaccion.Retiro,
                Monto = new Moneda { Valor = p_monto.Valor, Divisa = p_monto.Divisa },
                Fecha = DateTime.UtcNow,
                Concepto = $"Retirada de fondos hacia {p_destino.Trim()}",
                Monedero = monedero
            };
            await _unitOfWork.TransaccionRepository.SaveAsync(result);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Monedero.retirarSaldoWallet.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
