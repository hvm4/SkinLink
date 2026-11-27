// Fichero manual (no lo genera HARIA Web): modelos de las vistas de filtros y operaciones
// de ActivoDigital, ArticuloCatalogo y CajaMisteriosa.
using System.ComponentModel.DataAnnotations;
using ejemplo.Domain.Enums;

namespace Skinlink.Web.ViewModels;

/// <summary>Búsqueda de activos: filtros por rareza, colección, condición y en venta.</summary>
public sealed class ActivoBusquedaViewModel
{
    [Display(Name = "Rareza")]
    public Rareza? Rareza { get; set; }

    [Display(Name = "Colección")]
    public string? Coleccion { get; set; }

    [Display(Name = "Condición")]
    public Condicion? Condicion { get; set; }

    [Display(Name = "Usuario (Id)")]
    public int? UsuarioId { get; set; }

    [Display(Name = "En venta")]
    public bool EnVenta { get; set; } = true;

    /// <summary>Descripción del filtro aplicado; null si aún no se ha buscado.</summary>
    public string? FiltroAplicado { get; set; }

    public bool EsAdministrador { get; set; }
    public bool Autenticado { get; set; }

    public List<ActivoDigitalViewModel> Resultados { get; set; } = new();

    /// <summary>Ids de los resultados que son del usuario conectado.</summary>
    public HashSet<int> Propios { get; set; } = new();

    /// <summary>Ids de los resultados que el usuario conectado tiene en favoritos.</summary>
    public HashSet<int> Favoritos { get; set; } = new();
}

/// <summary>Búsqueda de cajas: activas o por rango de precio de apertura.</summary>
public sealed class CajaBusquedaViewModel
{
    [Display(Name = "Precio mínimo")]
    public double? PrecioMinimo { get; set; }

    [Display(Name = "Precio máximo")]
    public double? PrecioMaximo { get; set; }

    public string? FiltroAplicado { get; set; }

    public List<CajaMisteriosaViewModel> Resultados { get; set; } = new();

    /// <summary>Número de posibles recompensas de cada caja (id de caja → probabilidades).</summary>
    public Dictionary<int, int> Recompensas { get; set; } = new();
}

/// <summary>Panel de operaciones de un activo (bloquear, desbloquear, precio, autenticidad, reporte).</summary>
public sealed class ActivoOperacionesViewModel
{
    public ActivoDigitalViewModel Activo { get; set; } = new();

    public string? Propietario { get; set; }

    /// <summary>El usuario autenticado es el dueño del activo.</summary>
    public bool EsPropietario { get; set; }
    public bool EsAdministrador { get; set; }

    [Display(Name = "Nuevo precio estimado")]
    public double NuevoPrecioValor { get; set; }

    [Display(Name = "Divisa")]
    public string NuevoPrecioDivisa { get; set; } = "EUR";

    /// <summary>Resultado de verificarAutenticidad (null si no se ha consultado).</summary>
    public bool? Autentico { get; set; }

    /// <summary>Texto de generarReporteActividad (null si no se ha generado).</summary>
    public string? Reporte { get; set; }
}

/// <summary>Formulario de actualizarDescuentoArticulo.</summary>
public sealed class ArticuloDescuentoViewModel
{
    [Display(Name = "Artículo")]
    public int ArticuloId { get; set; }

    [Display(Name = "Descuento (%)")]
    [Range(0, 100)]
    public double Porcentaje { get; set; }

    [Display(Name = "Fin del descuento")]
    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = DateTime.Today.AddDays(7);

    public List<ArticuloCatalogoViewModel> Articulos { get; set; } = new();
}

/// <summary>Fila de la tabla de probabilidades de una caja (RF-20: legibles antes de abrir).</summary>
public sealed class ProbabilidadFilaViewModel
{
    public int ActivoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Coleccion { get; set; } = string.Empty;
    public Rareza Rareza { get; set; }
    public double Porcentaje { get; set; }
    public string? Imagen { get; set; }
}

/// <summary>Página de una caja: probabilidades, simulación (simularApertura) y apertura (abrirCajaSorpresa).</summary>
public sealed class CajaAperturaViewModel
{
    public CajaMisteriosaViewModel Caja { get; set; } = new();
    public List<ProbabilidadFilaViewModel> Probabilidades { get; set; } = new();
    public double SumaPorcentajes => Probabilidades.Sum(p => p.Porcentaje);

    public bool Autenticado { get; set; }
    public double? SaldoUsuario { get; set; }
    public string? DivisaUsuario { get; set; }

    /// <summary>Resultado de la última simulación o apertura (null si no hay).</summary>
    public ActivoDigitalViewModel? Resultado { get; set; }
    public bool ResultadoEsSimulacion { get; set; }
}
