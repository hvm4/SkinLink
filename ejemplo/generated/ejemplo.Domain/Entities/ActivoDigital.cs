using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ejemplo.Domain.ValueObjects;

namespace ejemplo.Domain.Entities;

public partial class ActivoDigital
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Coleccion { get; set; } = string.Empty;
    public Moneda PrecioEstimado { get; set; } = null!;
    public AtributosSkin Atributos { get; set; } = null!;
    public bool EnVenta { get; set; }
    public InventarioPrivado Inventario { get; set; } = null!;
    public Venta? Venta { get; set; }
    public IList<ProbabilidadDrop> Probabilidad { get; set; } = new List<ProbabilidadDrop>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ActivoDigital other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
