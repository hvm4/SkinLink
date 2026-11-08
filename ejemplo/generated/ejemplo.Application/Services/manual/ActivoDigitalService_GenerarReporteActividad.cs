using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ActivoDigital_generarReporteActividad) ENABLED START*/
        // references to other libraries
        using System.Globalization;
        using System.Text;
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ActivoDigitalService
{
    public async Task<string> GenerarReporteActividadAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ActivoDigital_generarReporteActividad) ENABLED START*/
        var activo = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException($"ActivoDigital con id {p_oid} no encontrado.");

        // Cargar relaciones en el contexto compartido (EF las enlaza)
        var todosLosActivos = await _repository.ReadAllAsync();
        var ventas = await _unitOfWork.VentaRepository.ReadAllAsync();
        var favoritos = await _unitOfWork.FavoritoRepository.ReadAllAsync();
        var drops = await _unitOfWork.ProbabilidadDropRepository.ReadAllAsync();

        CultureInfo ci = CultureInfo.InvariantCulture;
        string divisa = activo.PrecioEstimado?.Divisa ?? string.Empty;
        double precioEstimado = activo.PrecioEstimado?.Valor ?? 0d;

        var ventasActivo = ventas
            .Where(v => v.ActivoDigital?.Id == activo.Id)
            .OrderBy(v => v.FechaPublicacion)
            .ToList();
        int totalFavoritos = favoritos.Count(f => f.ActivoDigital?.Id == activo.Id);
        var dropsActivo = drops.Where(d => d.ActivoRecompensa?.Id == activo.Id).ToList();
        var coleccion = todosLosActivos
            .Where(a => string.Equals(a.Coleccion, activo.Coleccion, StringComparison.OrdinalIgnoreCase)
                        && a.PrecioEstimado != null)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"REPORTE DE ACTIVIDAD - {activo.Nombre} (id {activo.Id})");
        sb.AppendLine($"Colección: {activo.Coleccion}");
        sb.AppendLine($"Identificador único: {activo.IdentificadorUnico}");
        sb.AppendLine($"Precio estimado: {precioEstimado.ToString("F2", ci)} {divisa}");
        sb.AppendLine($"En venta: {(activo.EnVenta ? "Sí" : "No")}");

        // 1) Actividad de mercado y fluctuación del precio
        sb.AppendLine("-- Actividad de mercado --");
        sb.AppendLine($"Ventas registradas: {ventasActivo.Count}");
        foreach (EstadoVenta estado in Enum.GetValues<EstadoVenta>())
            sb.AppendLine($"  {estado}: {ventasActivo.Count(v => v.EstadoVenta == estado)}");

        if (ventasActivo.Count > 0)
        {
            double ultimoPrecio = ventasActivo[^1].PrecioVenta?.Valor ?? 0d;
            double minVenta = ventasActivo.Min(v => v.PrecioVenta?.Valor ?? 0d);
            double maxVenta = ventasActivo.Max(v => v.PrecioVenta?.Valor ?? 0d);
            sb.AppendLine($"Último precio de venta: {ultimoPrecio.ToString("F2", ci)} {divisa}");
            sb.AppendLine($"Precio de venta mínimo / máximo: {minVenta.ToString("F2", ci)} / {maxVenta.ToString("F2", ci)} {divisa}");
            if (precioEstimado > 0)
            {
                double variacion = (ultimoPrecio - precioEstimado) / precioEstimado * 100d;
                sb.AppendLine($"Variación del último precio de venta frente al estimado: {variacion.ToString("+0.00;-0.00;0.00", ci)}%");
            }
        }
        else
        {
            sb.AppendLine("Sin ventas registradas: no hay fluctuación de precio de venta que analizar.");
        }

        // 2) Interés de los usuarios
        sb.AppendLine("-- Interés de los usuarios --");
        sb.AppendLine($"Veces marcado como favorito: {totalFavoritos}");
        sb.AppendLine($"Cajas misteriosas que lo incluyen como recompensa: {dropsActivo.Count}");
        if (dropsActivo.Count > 0)
            sb.AppendLine($"Probabilidad media de drop: {dropsActivo.Average(d => d.PorcentajeDrop).ToString("F2", ci)}%");

        // 3) Posición dentro de su colección
        sb.AppendLine("-- Posición en su colección --");
        if (coleccion.Count > 0)
        {
            double media = coleccion.Average(a => a.PrecioEstimado.Valor);
            double min = coleccion.Min(a => a.PrecioEstimado.Valor);
            double max = coleccion.Max(a => a.PrecioEstimado.Valor);
            sb.AppendLine($"Activos en la colección: {coleccion.Count}");
            sb.AppendLine($"Precio medio / mínimo / máximo: {media.ToString("F2", ci)} / {min.ToString("F2", ci)} / {max.ToString("F2", ci)} {divisa}");
            if (media > 0)
            {
                double desv = (precioEstimado - media) / media * 100d;
                sb.AppendLine($"Desviación respecto a la media de la colección: {desv.ToString("+0.00;-0.00;0.00", ci)}%");
            }
        }

        return sb.ToString();
        /*PROTECTED REGION END*/
    }
}
