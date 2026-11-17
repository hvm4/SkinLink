// Fichero manual (no lo genera HARIA Web): vista del ReadFilter filtrarTransaccionesPorRangoFechas.
using System.ComponentModel.DataAnnotations;
using ejemplo.Domain.Enums;

namespace Skinlink.Web.ViewModels;

public sealed class TransaccionFiltroFechasViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public DateTime FechaFin { get; set; }

    public bool Buscado { get; set; }

    /// <summary>true si es un usuario normal: solo se le muestran los movimientos de su monedero.</summary>
    public bool SoloMisMovimientos { get; set; }

    public List<TransaccionViewModel> Resultados { get; set; } = new();

    public static bool EsEntrada(TipoTransaccion tipo) =>
        tipo is TipoTransaccion.Deposito or TipoTransaccion.CobroVenta;

    public double TotalEntradas => Resultados.Where(t => EsEntrada(t.Tipo)).Sum(t => t.MontoValor);
    public double TotalSalidas => Resultados.Where(t => !EsEntrada(t.Tipo)).Sum(t => t.MontoValor);
    public double Balance => TotalEntradas - TotalSalidas;
    public string Divisa => Resultados.FirstOrDefault()?.MontoDivisa ?? "EUR";
}