using Microsoft.AspNetCore.Mvc.Rendering;

namespace Skinlink.Web.ViewModels;

public sealed class OperacionMonederoViewModel
{
    public List<SelectListItem> Monederos { get; set; } = new();
    public int? MonederoId { get; set; }

    public bool MonederoFijo { get; set; }

    public string Divisa { get; set; } = "EUR";
}

public sealed class VentaOperacionItem
{
    public int VentaId { get; set; }
    public int ActivoId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public double Precio { get; set; }
    public string Divisa { get; set; } = "EUR";
}

public sealed class OperacionVentaViewModel
{
    public List<VentaOperacionItem> Ventas { get; set; } = new();
    public int? VentaId { get; set; }

    public bool EsAdministrador { get; set; }

    public int? CompradorId { get; set; }
    public string CompradorNombre { get; set; } = string.Empty;

    public List<SelectListItem> Compradores { get; set; } = new();
}
