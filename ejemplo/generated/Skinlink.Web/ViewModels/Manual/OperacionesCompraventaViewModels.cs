// Fichero manual (no lo genera HARIA Web): modelos de las vistas de operaciones de monedero y venta.
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Skinlink.Web.ViewModels;

public sealed class OperacionMonederoViewModel
{
    public List<SelectListItem> Monederos { get; set; } = new();
    public int? MonederoId { get; set; }

    /// <summary>true si es un usuario normal: solo puede operar con su propio monedero.</summary>
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

    /// <summary>Comprador (solo ProcesarCompraSkin). Un usuario normal siempre es él mismo.</summary>
    public int? CompradorId { get; set; }
    public string CompradorNombre { get; set; } = string.Empty;

    /// <summary>Lista de compradores posibles; solo se rellena para administradores.</summary>
    public List<SelectListItem> Compradores { get; set; } = new();
}
