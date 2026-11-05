using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ejemplo.Domain.ValueObjects;

namespace ejemplo.Domain.Entities;

public partial class CajaMisteriosa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Moneda PrecioApertura { get; set; } = null!;
    public bool Activa { get; set; }
    public IList<ProbabilidadDrop> Probabilidades { get; set; } = new List<ProbabilidadDrop>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is CajaMisteriosa other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
