// Fichero manual (no lo genera HARIA Web): vistas de los ReadFilter filtrarVentasPorEstado y filtrarVentasPorPrecioMaximo.
using System.ComponentModel.DataAnnotations;
using ejemplo.Domain.Enums;

namespace Skinlink.Web.ViewModels;

public sealed class VentaFiltroEstadoViewModel
{
    [Display(Name = "Estado de la venta")]
    public EstadoVenta Estado { get; set; } = EstadoVenta.Disponible;

    public bool Buscado { get; set; }

    public List<VentaViewModel> Resultados { get; set; } = new();
}

public sealed class VentaFiltroPrecioViewModel
{
    /// <summary>Texto tal y como lo escribe el usuario (admite coma o punto decimal).</summary>
    [Display(Name = "Precio máximo")]
    public string PrecioMaximo { get; set; } = string.Empty;

    public bool Buscado { get; set; }

    public List<VentaViewModel> Resultados { get; set; } = new();
}
