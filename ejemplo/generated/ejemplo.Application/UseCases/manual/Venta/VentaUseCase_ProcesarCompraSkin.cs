using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_Venta_procesarCompraSkin) ENABLED START*/
        using System;
        using System.Linq;
        using ejemplo.Domain.ValueObjects;
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.Venta;

public sealed partial class VentaUseCase
{
    public async Task ProcesarCompraSkinAsync(int p_oid, int p_comprador_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_Venta_procesarCompraSkin) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        VentaService ventaService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("VentaUseCase.ProcesarCompraSkinAsync");
            ventaService = new VentaService(_unitOfWork);

            var venta = await _unitOfWork.VentaRepository.ReadAsync(p_oid)
                ?? throw new ModelException($"Venta con id {p_oid} no encontrada.");

            if (venta.EstadoVenta != EstadoVenta.Disponible)
                throw new ModelException("El activo no está disponible para su compra.");

            var comprador = await _unitOfWork.UsuarioRepository.ReadAsync(p_comprador_oid)
                ?? throw new ModelException($"Comprador con id {p_comprador_oid} no encontrado.");

            if (comprador.Estado != EstadoCuenta.Activo)
                throw new ModelException("La cuenta del comprador no está activa.");

            // Cargar datos relacionales necesarios en el contexto de EF
            await _unitOfWork.UsuarioRepository.ReadAllAsync();
            await _unitOfWork.MonederoRepository.ReadAllAsync();
            await _unitOfWork.InventarioPrivadoRepository.ReadAllAsync();
            await _unitOfWork.ActivoDigitalRepository.ReadAllAsync();

            var vendedor = venta.Vendedor
                ?? throw new ModelException("La venta no tiene un vendedor asociado.");

            if (comprador.Id == vendedor.Id)
                throw new ModelException("El comprador no puede ser el mismo que el vendedor.");

            var activo = venta.ActivoDigital
                ?? throw new ModelException("La venta no tiene un activo digital asociado.");

            var monederoComprador = comprador.Monedero
                ?? throw new ModelException("El comprador no tiene un monedero asociado.");

            var monederoVendedor = vendedor.Monedero
                ?? throw new ModelException("El vendedor no tiene un monedero asociado.");

            var inventarioComprador = comprador.Inventario
                ?? throw new ModelException("El comprador no tiene un inventario asociado.");

            var precio = venta.PrecioVenta;
            if (monederoComprador.SaldoDisponible.Divisa != precio.Divisa)
                throw new ModelException("La divisa del monedero del comprador no coincide con la del precio de venta.");

            if (monederoComprador.SaldoDisponible.Valor < precio.Valor)
                throw new ModelException("Saldo insuficiente en el monedero del comprador.");

            // 1) Descontar importe al comprador
            monederoComprador.SaldoDisponible = new Moneda
            {
                Valor = monederoComprador.SaldoDisponible.Valor - precio.Valor,
                Divisa = monederoComprador.SaldoDisponible.Divisa
            };
            await _unitOfWork.MonederoRepository.SaveAsync(monederoComprador);

            // 2) Acreditar fondos al vendedor
            monederoVendedor.SaldoDisponible = new Moneda
            {
                Valor = monederoVendedor.SaldoDisponible.Valor + precio.Valor,
                Divisa = monederoVendedor.SaldoDisponible.Divisa
            };
            await _unitOfWork.MonederoRepository.SaveAsync(monederoVendedor);

            // 3) Registrar transacciones de cobro y pago
            var transaccionPagoComprador = new Transaccion
            {
                Tipo = TipoTransaccion.PagoVenta,
                Monto = new Moneda { Valor = precio.Valor, Divisa = precio.Divisa },
                Fecha = DateTime.UtcNow,
                Concepto = $"Pago por compra de activo '{activo.Nombre}' (Venta #{venta.Id})",
                Monedero = monederoComprador
            };
            await _unitOfWork.TransaccionRepository.SaveAsync(transaccionPagoComprador);

            var transaccionCobroVendedor = new Transaccion
            {
                Tipo = TipoTransaccion.CobroVenta,
                Monto = new Moneda { Valor = precio.Valor, Divisa = precio.Divisa },
                Fecha = DateTime.UtcNow,
                Concepto = $"Cobro por venta de activo '{activo.Nombre}' a usuario '{comprador.NombreUsuario}'",
                Monedero = monederoVendedor
            };
            await _unitOfWork.TransaccionRepository.SaveAsync(transaccionCobroVendedor);

            // 4) Transferir propiedad del activo digital al inventario del comprador
            activo.Inventario = inventarioComprador;
            activo.EnVenta = false;
            await _unitOfWork.ActivoDigitalRepository.SaveAsync(activo);

            // 5) Actualizar la venta: comprador, estado y transacciones
            venta.Comprador = comprador;
            venta.EstadoVenta = EstadoVenta.Completada;
            venta.TransaccionPago = transaccionPagoComprador;
            venta.TransaccionCobro = transaccionCobroVendedor;
            await _unitOfWork.VentaRepository.SaveAsync(venta);

            // 6) Incrementar XP al comprador tras la compra (CU-12 / RF-04)
            comprador.Experiencia += 50;
            // Verificar umbral de nivel si procede (cada 100 XP sube un nivel)
            int nuevoNivel = (comprador.Experiencia / 100) + 1;
            if (nuevoNivel > comprador.Nivel)
            {
                comprador.Nivel = nuevoNivel;
            }
            await _unitOfWork.UsuarioRepository.SaveAsync(comprador);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Venta.procesarCompraSkin.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
