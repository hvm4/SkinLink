using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ejemplo.Domain.Entities;

public partial class ResenaVendedor
{
    public int Id { get; set; }
    public int Puntuacion { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public bool Oculta { get; set; }
    public Usuario Comprador { get; set; } = null!;
    public Usuario Vendedor { get; set; } = null!;

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ResenaVendedor other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
