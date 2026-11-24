// Fichero manual (no lo genera HARIA Web): ViewModels de las vistas de operaciones de Usuario/Administrador (Miembro 1).
using System.ComponentModel.DataAnnotations;
using ejemplo.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Skinlink.Web.ViewModels;

/// <summary>Ficha de un usuario con los datos de la herencia Usuario &lt;- Administrador.</summary>
public sealed class UsuarioPerfilViewModel
{
    public UsuarioViewModel Usuario { get; set; } = new();
    public bool EsAdministrador { get; set; }
    public string? CodigoAuditor { get; set; }
    public double SaldoDisponible { get; set; }
    public double SaldoPendiente { get; set; }
    public string Divisa { get; set; } = string.Empty;
    public bool InventarioPublico { get; set; }
    public bool EsMiPerfil { get; set; }
    public bool YaReclamadoHoy { get; set; }
}

/// <summary>Formulario y resultado del filtro filtrarUsuariosPorEstado.</summary>
public sealed class UsuarioFiltroEstadoViewModel
{
    [Display(Name = "Estado de la cuenta")]
    public EstadoCuenta Estado { get; set; } = EstadoCuenta.Activo;
    public bool Buscado { get; set; }
    public List<UsuarioViewModel> Resultados { get; set; } = new();
    public HashSet<int> AdministradorIds { get; set; } = new();
}

/// <summary>Formulario de la operación custom sumarExperiencia.</summary>
public sealed class SumarExperienciaViewModel
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public int NivelActual { get; set; }
    public int ExperienciaActual { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Los puntos deben ser positivos.")]
    [Display(Name = "Puntos de experiencia")]
    public int Puntos { get; set; } = 50;
}

/// <summary>Formulario de la operación customTransaction reclamarRecompensaDiaria.</summary>
public sealed class ReclamarRecompensaViewModel
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public bool YaReclamadoHoy { get; set; }

    [Display(Name = "Caja gratuita")]
    public int? CajaId { get; set; }
    public List<SelectListItem> Cajas { get; set; } = new();
}

/// <summary>Formulario de la operación custom actualizarEstadoKYC (solo administradores).</summary>
public sealed class ActualizarEstadoKYCViewModel
{
    [Display(Name = "Usuario")]
    public int? UsuarioId { get; set; }

    [Display(Name = "Nuevo estado")]
    public EstadoCuenta NuevoEstado { get; set; } = EstadoCuenta.Suspendido;

    public List<SelectListItem> Usuarios { get; set; } = new();
}
