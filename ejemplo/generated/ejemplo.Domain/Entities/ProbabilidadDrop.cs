using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ejemplo.Domain.Entities;

public partial class ProbabilidadDrop
{
    public int Id { get; set; }
    public double PorcentajeDrop { get; set; }
    public CajaMisteriosa Caja { get; set; } = null!;
    public ActivoDigital ActivoRecompensa { get; set; } = null!;

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ProbabilidadDrop other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
