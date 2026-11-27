// Fichero manual (no lo genera HARIA Web): pantalla "Poner a la venta" del usuario.
namespace Skinlink.Web.ViewModels;

public sealed class PonerALaVentaViewModel
{
    /// <summary>Activos del inventario del usuario, con el motivo si no se pueden vender ahora.</summary>
    public List<ActivoParaVenderItem> Activos { get; set; } = new();

    /// <summary>Ventas del usuario que están publicadas (Disponible).</summary>
    public List<VentaPublicadaItem> VentasPublicadas { get; set; } = new();

    public int? ActivoId { get; set; }
}

public sealed class ActivoParaVenderItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public double PrecioEstimado { get; set; }
    public string Divisa { get; set; } = "EUR";

    /// <summary>Null si se puede poner a la venta; si no, el motivo para enseñarlo al usuario.</summary>
    public string? Motivo { get; set; }
}

public sealed class VentaPublicadaItem
{
    public int VentaId { get; set; }
    public string Activo { get; set; } = string.Empty;
    public double Precio { get; set; }
    public string Divisa { get; set; } = "EUR";
    public DateTime FechaPublicacion { get; set; }
}
