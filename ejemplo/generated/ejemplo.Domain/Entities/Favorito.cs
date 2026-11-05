using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ejemplo.Domain.Entities;

public partial class Favorito
{
    public int Id { get; set; }
    public DateTime FechaMarcado { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public ActivoDigital ActivoDigital { get; set; } = null!;

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Favorito other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
