using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_CajaMisteriosa_abrirCajaSorpresa) ENABLED START*/
        // references to other libraries
        using ejemplo.Domain.ValueObjects;
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.CajaMisteriosa;

public sealed partial class CajaMisteriosaUseCase
{
    public async Task<global::ejemplo.Domain.Entities.ActivoDigital> AbrirCajaSorpresaAsync(int p_oid, int p_usuario_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_CajaMisteriosa_abrirCajaSorpresa) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        CajaMisteriosaService cajaMisteriosaService = null!;
        global::ejemplo.Domain.Entities.ActivoDigital result = default(global::ejemplo.Domain.Entities.ActivoDigital)!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("CajaMisteriosaUseCase.AbrirCajaSorpresaAsync");
            cajaMisteriosaService = new CajaMisteriosaService(_unitOfWork);

            var caja = await _unitOfWork.CajaMisteriosaRepository.ReadAsync(p_oid)
                ?? throw new ModelException("CajaMisteriosa not found.");
            if (!caja.Activa)
                throw new ModelException("La caja no está disponible.");

            var usuario = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
                ?? throw new ModelException("Usuario not found.");
            if (usuario.Estado != EstadoCuenta.Activo)
                throw new ModelException("La cuenta del usuario no está activa.");

            // Cargar relaciones en el contexto compartido (EF las enlaza)
            await _unitOfWork.ProbabilidadDropRepository.ReadAllAsync();
            await _unitOfWork.ActivoDigitalRepository.ReadAllAsync();
            await _unitOfWork.InventarioPrivadoRepository.ReadAllAsync();
            await _unitOfWork.MonederoRepository.ReadAllAsync();

            var monedero = usuario.Monedero
                ?? throw new ModelException("El usuario no tiene monedero.");
            var inventario = usuario.Inventario
                ?? throw new ModelException("El usuario no tiene inventario.");

            // 1) Descontar el coste de la caja
            var coste = caja.PrecioApertura;
            if (monedero.SaldoDisponible.Divisa != coste.Divisa)
                throw new ModelException("La divisa del monedero no coincide con la de la caja.");
            if (monedero.SaldoDisponible.Valor < coste.Valor)
                throw new ModelException("Saldo insuficiente para abrir la caja.");

            monedero.SaldoDisponible = new Moneda
            {
                Valor = monedero.SaldoDisponible.Valor - coste.Valor,
                Divisa = monedero.SaldoDisponible.Divisa
            };
            await _unitOfWork.MonederoRepository.SaveAsync(monedero);

            await _unitOfWork.TransaccionRepository.SaveAsync(new Transaccion
            {
                Tipo = TipoTransaccion.CosteCaja,
                Monto = new Moneda { Valor = coste.Valor, Divisa = coste.Divisa },
                Fecha = DateTime.UtcNow,
                Concepto = $"Apertura de caja '{caja.Nombre}'",
                Monedero = monedero
            });

            // 2) Algoritmo de probabilidad (ruleta por peso de drop)
            var drops = caja.Probabilidades.Where(d => d.PorcentajeDrop > 0).ToList();
            if (drops.Count == 0)
                throw new ModelException("La caja no tiene probabilidades de obtención configuradas.");

            double total = drops.Sum(d => d.PorcentajeDrop);
            double tirada = Random.Shared.NextDouble() * total;
            double acumulado = 0;
            var elegido = drops[^1];
            foreach (var d in drops)
            {
                acumulado += d.PorcentajeDrop;
                if (tirada < acumulado) { elegido = d; break; }
            }

            var activoBase = elegido.ActivoRecompensa
                ?? throw new ModelException("La probabilidad elegida no tiene activo de recompensa asociado.");

            // 3) Generar el nuevo activo en el inventario del usuario a partir del activo base
            double floatVal = Random.Shared.NextDouble();
            result = new global::ejemplo.Domain.Entities.ActivoDigital
            {
                Nombre = activoBase.Nombre,
                Coleccion = activoBase.Coleccion,
                PrecioEstimado = new Moneda
                {
                    Valor = activoBase.PrecioEstimado?.Valor ?? 0,
                    Divisa = activoBase.PrecioEstimado?.Divisa ?? "EUR"
                },
                Atributos = new AtributosSkin
                {
                    Rareza = activoBase.Atributos?.Rareza ?? Rareza.Comun,
                    Condicion = floatVal switch
                    {
                        < 0.07 => Condicion.RecienFabricado,
                        < 0.15 => Condicion.DesgasteMinimo,
                        < 0.38 => Condicion.AlgoDesgastado,
                        < 0.45 => Condicion.BastanteDesgastado,
                        _ => Condicion.Deplorable
                    },
                    FloatVal = floatVal,
                    Patron = Random.Shared.Next(0, 1000)
                },
                EnVenta = false,
                IdentificadorUnico = Guid.NewGuid().ToString("N"),
                Inventario = inventario
            };
            await _unitOfWork.ActivoDigitalRepository.SaveAsync(result);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case CajaMisteriosa.abrirCajaSorpresa.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}
