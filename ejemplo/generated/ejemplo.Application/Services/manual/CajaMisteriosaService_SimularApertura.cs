using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_CajaMisteriosa_simularApertura) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class CajaMisteriosaService
{
    public async Task<ActivoDigital> SimularAperturaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_CajaMisteriosa_simularApertura) ENABLED START*/
        // Modo prueba (RF-22): simula la apertura con el mismo algoritmo de probabilidades que
        // abrirCajaSorpresa, pero sin cobrar, sin crear el activo en ningún inventario y sin guardar nada.
        // Devuelve un ActivoDigital NO persistido (Id = 0) para mostrar el resultado.
        var caja = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("CajaMisteriosa not found.");
        if (!caja.Activa)
            throw new ModelException("La caja no está disponible.");

        var drops = caja.Probabilidades.Where(d => d.PorcentajeDrop > 0 && d.ActivoRecompensa is not null).ToList();
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

        var activoBase = elegido.ActivoRecompensa;
        double floatVal = Random.Shared.NextDouble();
        return new ActivoDigital
        {
            Nombre = activoBase.Nombre,
            Coleccion = activoBase.Coleccion,
            PrecioEstimado = new global::ejemplo.Domain.ValueObjects.Moneda
            {
                Valor = activoBase.PrecioEstimado?.Valor ?? 0,
                Divisa = activoBase.PrecioEstimado?.Divisa ?? "EUR"
            },
            Atributos = new global::ejemplo.Domain.ValueObjects.AtributosSkin
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
            IdentificadorUnico = "SIMULACION",
            Imagen = activoBase.Imagen
        };
        /*PROTECTED REGION END*/
    }
}
