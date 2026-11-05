using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ejemplo.Domain.Enums;

namespace ejemplo.Domain.Entities;

public partial class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public EstadoCuenta Estado { get; set; }
    public int Nivel { get; set; }
    public int Experiencia { get; set; }
    public DateTime FechaUltimoLogin { get; set; }
    public bool TemaModoOscuro { get; set; }
    public string Idioma { get; set; } = string.Empty;
    public Monedero Monedero { get; set; } = null!;
    public InventarioPrivado Inventario { get; set; } = null!;
    public IList<Venta> VentasPublicadas { get; set; } = new List<Venta>();
    public IList<Venta> ComprasRealizadas { get; set; } = new List<Venta>();
    public IList<ResenaVendedor> ResenasRealizadas { get; set; } = new List<ResenaVendedor>();
    public IList<ResenaVendedor> ResenasRecibidas { get; set; } = new List<ResenaVendedor>();
    public IList<Favorito> Favoritos { get; set; } = new List<Favorito>();
    public IList<Intercambio> IntercambiosPropuestos { get; set; } = new List<Intercambio>();
    public IList<Intercambio> IntercambiosRecibidos { get; set; } = new List<Intercambio>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Usuario other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
