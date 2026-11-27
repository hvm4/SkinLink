using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ejemplo.Domain.Enums;

namespace Skinlink.Web.ViewModels;

// Reseñas (filtrarResenasVisiblesPorVendedor, ocultarResena, moderarResena)

public sealed class ResenasVendedorPaginaViewModel
{
    [Display(Name = "Vendedor")]
    public int? VendedorId { get; set; }

    public string? VendedorNombre { get; set; }

    public List<SelectListItem> Vendedores { get; set; } = new();

    public List<ResenaVendedorViewModel> Resenas { get; set; } = new();

    public double? NotaMedia { get; set; }

    public bool PuedeOcultar { get; set; }

    public bool EsAdministrador { get; set; }
}

public sealed class ModeracionResenasViewModel
{
    public List<ResenaVendedorViewModel> Resenas { get; set; } = new();
}

// Intercambios (iniciarIntercambio, aceptarIntercambio, cancelarIntercambio)

public sealed class ActivoIntercambioViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Coleccion { get; set; } = string.Empty;
    public Rareza Rareza { get; set; }
    public Condicion Condicion { get; set; }
    public double Precio { get; set; }
    public string Divisa { get; set; } = "EUR";
}

public sealed class IntercambioResumenViewModel
{
    public int Id { get; set; }
    public EstadoIntercambio Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string ProponenteNombre { get; set; } = string.Empty;
    public string ReceptorNombre { get; set; } = string.Empty;
    public List<ActivoIntercambioViewModel> Ofrecidos { get; set; } = new();
    public List<ActivoIntercambioViewModel> Solicitados { get; set; } = new();
    public bool SoyReceptor { get; set; }
    public bool SoyProponente { get; set; }
}

public sealed class MisIntercambiosViewModel
{
    public List<IntercambioResumenViewModel> Recibidos { get; set; } = new();
    public List<IntercambioResumenViewModel> Enviados { get; set; } = new();
    public List<IntercambioResumenViewModel> Historial { get; set; } = new();
}

public sealed class ProponerIntercambioViewModel
{
    [Display(Name = "Usuario con quien intercambiar")]
    public int? ReceptorId { get; set; }

    public string? ReceptorNombre { get; set; }

    public List<SelectListItem> Receptores { get; set; } = new();

    public List<ActivoIntercambioViewModel> MisActivos { get; set; } = new();

    public List<ActivoIntercambioViewModel> ActivosReceptor { get; set; } = new();
}

// Favoritos (marcarComoFavorito)

public sealed class FavoritoFilaViewModel
{
    public int FavoritoId { get; set; }
    public int ActivoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Coleccion { get; set; } = string.Empty;
    public Rareza Rareza { get; set; }
    public Condicion Condicion { get; set; }
    public double Precio { get; set; }
    public string Divisa { get; set; } = "EUR";
    public bool EnVenta { get; set; }
    public DateTime FechaMarcado { get; set; }
}

public sealed class MisFavoritosViewModel
{
    public List<FavoritoFilaViewModel> Favoritos { get; set; } = new();
}

// Inventario (cambiarVisibilidadInventario)

public sealed class VisibilidadInventarioViewModel
{
    public int InventarioId { get; set; }
    public string Propietario { get; set; } = string.Empty;
    public bool EsPublico { get; set; }
    public int NumeroActivos { get; set; }
    public double ValorEstimado { get; set; }
    public string Divisa { get; set; } = "EUR";

    public List<VisibilidadInventarioViewModel> Otros { get; set; } = new();
}

// Configuración del sistema (calcularComisionMercado)

public sealed class ComisionMercadoViewModel
{
    public int ConfiguracionId { get; set; }

    [Display(Name = "Precio de venta (EUR)")]
    [Range(0.01, 1_000_000, ErrorMessage = "El precio debe ser mayor que 0.")]
    public double? Precio { get; set; }

    public double LimitePrecioBajo { get; set; }
    public double LimitePrecioMedio { get; set; }
    public double ComisionPrecioBajo { get; set; }
    public double ComisionPrecioMedio { get; set; }
    public double ComisionPrecioAlto { get; set; }

    public double? Porcentaje { get; set; }
    public double? ImporteComision { get; set; }
    public double? ImporteNeto { get; set; }
}
