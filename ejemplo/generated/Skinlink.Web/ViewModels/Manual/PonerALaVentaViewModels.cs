namespace Skinlink.Web.ViewModels;

public sealed class PonerALaVentaViewModel
{
    public List<ActivoParaVenderItem> Activos { get; set; } = new();

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
