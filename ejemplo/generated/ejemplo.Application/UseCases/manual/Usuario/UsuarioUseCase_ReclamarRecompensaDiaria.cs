using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_Usuario_reclamarRecompensaDiaria) ENABLED START*/
        using System;
        using System.Linq;
        using ejemplo.Domain.ValueObjects;
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.Usuario;

public sealed partial class UsuarioUseCase
{
    public async Task<global::ejemplo.Domain.Entities.ActivoDigital> ReclamarRecompensaDiariaAsync(int p_oid, int p_caja_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_Usuario_reclamarRecompensaDiaria) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        UsuarioService usuarioService = null!;
        global::ejemplo.Domain.Entities.ActivoDigital result = default(global::ejemplo.Domain.Entities.ActivoDigital)!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("UsuarioUseCase.ReclamarRecompensaDiariaAsync");
            usuarioService = new UsuarioService(_unitOfWork);

            var usuario = await _unitOfWork.UsuarioRepository.ReadAsync(p_oid)
                ?? throw new ModelException($"Usuario con id {p_oid} no encontrado.");

            if (usuario.Estado != EstadoCuenta.Activo)
                throw new ModelException("La cuenta del usuario no está activa.");

            // Validar si ya reclamó hoy (bonificación diaria / primer acceso del día)
            DateTime ahora = DateTime.UtcNow;
            if (usuario.FechaUltimaRecompensa.HasValue &&
                usuario.FechaUltimaRecompensa.Value.Date == ahora.Date)
            {
                throw new ModelException("El usuario ya ha reclamado su bonificación diaria hoy.");
            }

            var caja = await _unitOfWork.CajaMisteriosaRepository.ReadAsync(p_caja_oid)
                ?? throw new ModelException($"Caja misteriosa con id {p_caja_oid} no encontrada.");

            if (!caja.Activa)
                throw new ModelException("La caja seleccionada no está activa.");

            // La recompensa diaria es gratuita: solo se admiten cajas sin coste de apertura.
            if (caja.PrecioApertura != null && caja.PrecioApertura.Valor > 0)
                throw new ModelException("La recompensa diaria solo puede canjearse en cajas gratuitas (precio de apertura 0).");

            // Cargar colecciones para enlaces de EF
            await _unitOfWork.ProbabilidadDropRepository.ReadAllAsync();
            await _unitOfWork.ActivoDigitalRepository.ReadAllAsync();
            await _unitOfWork.InventarioPrivadoRepository.ReadAllAsync();
            await _unitOfWork.MonederoRepository.ReadAllAsync();

            var inventario = usuario.Inventario
                ?? throw new ModelException("El usuario no tiene un inventario asociado.");

            // Seleccionar drop aleatorio según probabilidades de la caja
            var drops = caja.Probabilidades.Where(d => d.PorcentajeDrop > 0).ToList();
            if (drops.Count == 0)
                throw new ModelException("La caja no tiene probabilidades de recompensa configuradas.");

            double total = drops.Sum(d => d.PorcentajeDrop);
            double tirada = Random.Shared.NextDouble() * total;
            double acumulado = 0;
            var dropElegido = drops[^1];
            foreach (var d in drops)
            {
                acumulado += d.PorcentajeDrop;
                if (tirada < acumulado)
                {
                    dropElegido = d;
                    break;
                }
            }

            var activoBase = dropElegido.ActivoRecompensa
                ?? throw new ModelException("La probabilidad seleccionada no tiene activo de recompensa asociado.");

            // Generar nuevo activo en el inventario del usuario a partir del artículo base
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

            // Actualizar fecha de reclamo y otorgar experiencia
            usuario.FechaUltimaRecompensa = ahora;
            usuario.Experiencia += 25; // Bonificación de XP diaria
            int nuevoNivel = (usuario.Experiencia / 100) + 1;
            if (nuevoNivel > usuario.Nivel)
            {
                usuario.Nivel = nuevoNivel;
            }
            await _unitOfWork.UsuarioRepository.SaveAsync(usuario);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Usuario.reclamarRecompensaDiaria.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }
}