using System.ComponentModel.DataAnnotations;
using ejemplo.Domain.Enums;

namespace Skinlink.Web.ViewModels;

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

    public string? FiltroAplicado { get; set; }

    public bool EsAdministrador { get; set; }
    public bool Autenticado { get; set; }

    public List<ActivoDigitalViewModel> Resultados { get; set; } = new();

    public HashSet<int> Propios { get; set; } = new();

    public HashSet<int> Favoritos { get; set; } = new();
}

public sealed class CajaBusquedaViewModel
{
    [Display(Name = "Precio mínimo")]
    public double? PrecioMinimo { get; set; }

    [Display(Name = "Precio máximo")]
    public double? PrecioMaximo { get; set; }

    public string? FiltroAplicado { get; set; }

    public List<CajaMisteriosaViewModel> Resultados { get; set; } = new();

    public Dictionary<int, int> Recompensas { get; set; } = new();
}

public sealed class ActivoOperacionesViewModel
{
    public ActivoDigitalViewModel Activo { get; set; } = new();

    public string? Propietario { get; set; }

    public bool EsPropietario { get; set; }
    public bool EsAdministrador { get; set; }

    [Display(Name = "Nuevo precio estimado")]
    public double NuevoPrecioValor { get; set; }

    [Display(Name = "Divisa")]
    public string NuevoPrecioDivisa { get; set; } = "EUR";

    public bool? Autentico { get; set; }

    public string? Reporte { get; set; }
}

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

public sealed class ProbabilidadFilaViewModel
{
    public int ActivoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Coleccion { get; set; } = string.Empty;
    public Rareza Rareza { get; set; }
    public double Porcentaje { get; set; }
    public string? Imagen { get; set; }
}

public sealed class CajaAperturaViewModel
{
    public CajaMisteriosaViewModel Caja { get; set; } = new();
    public List<ProbabilidadFilaViewModel> Probabilidades { get; set; } = new();
    public double SumaPorcentajes => Probabilidades.Sum(p => p.Porcentaje);

    public bool Autenticado { get; set; }
    public double? SaldoUsuario { get; set; }
    public string? DivisaUsuario { get; set; }

    public ActivoDigitalViewModel? Resultado { get; set; }
    public bool ResultadoEsSimulacion { get; set; }
}
