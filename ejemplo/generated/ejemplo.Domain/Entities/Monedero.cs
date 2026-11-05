using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ejemplo.Domain.ValueObjects;

namespace ejemplo.Domain.Entities;

public partial class Monedero
{
    public int Id { get; set; }
    public Moneda SaldoDisponible { get; set; } = null!;
    public Moneda SaldoPendiente { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public IList<Transaccion> Transacciones { get; set; } = new List<Transaccion>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Monedero other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
