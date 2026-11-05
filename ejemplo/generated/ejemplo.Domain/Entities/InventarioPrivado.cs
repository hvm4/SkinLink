using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ejemplo.Domain.ValueObjects;

namespace ejemplo.Domain.Entities;

public partial class InventarioPrivado
{
    public int Id { get; set; }
    public Moneda ValorEstimadoTotal { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public IList<ActivoDigital> Activos { get; set; } = new List<ActivoDigital>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is InventarioPrivado other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
