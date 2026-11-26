// Fichero manual (no lo genera HARIA Web) — Miembro 4: social y sistema.
// Modelos de las vistas de reseñas, intercambios, favoritos, visibilidad del inventario
// y cálculo de la comisión de mercado.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ejemplo.Domain.Enums;

namespace Skinlink.Web.ViewModels;

// ---------------------------------------------------------------------------
// Reseñas (filtrarResenasVisiblesPorVendedor, ocultarResena, moderarResena)
// ---------------------------------------------------------------------------

/// <summary>Reputación pública de un vendedor: sus reseñas visibles y la nota media.</summary>
public sealed class ResenasVendedorPaginaViewModel
{
    [Display(Name = "Vendedor")]
    public int? VendedorId { get; set; }

    public string? VendedorNombre { get; set; }

    public List<SelectListItem> Vendedores { get; set; } = new();

    public List<ResenaVendedorViewModel> Resenas { get; set; } = new();

    /// <summary>Media de las puntuaciones visibles (1 a 5); null si el vendedor no tiene reseñas.</summary>
    public double? NotaMedia { get; set; }

    /// <summary>El vendedor (o un administrador) puede ocultar reseñas de este perfil.</summary>
    public bool PuedeOcultar { get; set; }

    public bool EsAdministrador { get; set; }
}

/// <summary>Panel de moderación del administrador: todas las reseñas, visibles y ocultas.</summary>
public sealed class ModeracionResenasViewModel
{
    public List<ResenaVendedorViewModel> Resenas { get; set; } = new();
}

// ---------------------------------------------------------------------------
// Intercambios (iniciarIntercambio, aceptarIntercambio, cancelarIntercambio)
// ---------------------------------------------------------------------------

/// <summary>Un activo que se muestra (o se puede elegir) en un intercambio.</summary>
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

/// <summary>Resumen de un intercambio desde el punto de vista del usuario conectado.</summary>
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

/// <summary>Página "Mis intercambios": recibidos pendientes, enviados pendientes e historial.</summary>
public sealed class MisIntercambiosViewModel
{
    public List<IntercambioResumenViewModel> Recibidos { get; set; } = new();
    public List<IntercambioResumenViewModel> Enviados { get; set; } = new();
    public List<IntercambioResumenViewModel> Historial { get; set; } = new();
}

/// <summary>Formulario para proponer un intercambio a otro usuario.</summary>
public sealed class ProponerIntercambioViewModel
{
    [Display(Name = "Usuario con quien intercambiar")]
    public int? ReceptorId { get; set; }

    public string? ReceptorNombre { get; set; }

    public List<SelectListItem> Receptores { get; set; } = new();

    /// <summary>Activos propios que se pueden ofrecer (ni en venta ni en otro intercambio).</summary>
    public List<ActivoIntercambioViewModel> MisActivos { get; set; } = new();

    /// <summary>Activos del receptor que se le pueden pedir.</summary>
    public List<ActivoIntercambioViewModel> ActivosReceptor { get; set; } = new();
}

// ---------------------------------------------------------------------------
// Favoritos (marcarComoFavorito)
// ---------------------------------------------------------------------------

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

// ---------------------------------------------------------------------------
// Inventario (cambiarVisibilidadInventario)
// ---------------------------------------------------------------------------

public sealed class VisibilidadInventarioViewModel
{
    public int InventarioId { get; set; }
    public string Propietario { get; set; } = string.Empty;
    public bool EsPublico { get; set; }
    public int NumeroActivos { get; set; }
    public double ValorEstimado { get; set; }
    public string Divisa { get; set; } = "EUR";

    /// <summary>Solo para el administrador: inventarios de todos los usuarios.</summary>
    public List<VisibilidadInventarioViewModel> Otros { get; set; } = new();
}

// ---------------------------------------------------------------------------
// Configuración del sistema (calcularComisionMercado)
// ---------------------------------------------------------------------------

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

    /// <summary>Resultado del cálculo: porcentaje aplicado, importe de la comisión y lo que recibe el vendedor.</summary>
    public double? Porcentaje { get; set; }
    public double? ImporteComision { get; set; }
    public double? ImporteNeto { get; set; }
}
