using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ConfiguracionSistema_calcularComisionMercado) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ConfiguracionSistemaService
{
    public async Task<double> CalcularComisionMercadoAsync(int p_oid, double p_precio)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ConfiguracionSistema_calcularComisionMercado) ENABLED START*/
        if (double.IsNaN(p_precio) || double.IsInfinity(p_precio) || p_precio <= 0)
            throw new ModelException("El precio debe ser mayor que cero.");

        var configuracion = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException($"ConfiguracionSistema con id {p_oid} no encontrada.");

        // Devuelve el porcentaje de comisión (p. ej. 5 = 5 %) según el rango de precio (límites inclusivos).
        if (p_precio <= configuracion.LimitePrecioBajo)
            return configuracion.ComisionPrecioBajo;

        if (p_precio <= configuracion.LimitePrecioMedio)
            return configuracion.ComisionPrecioMedio;

        return configuracion.ComisionPrecioAlto;
        /*PROTECTED REGION END*/
    }
}
