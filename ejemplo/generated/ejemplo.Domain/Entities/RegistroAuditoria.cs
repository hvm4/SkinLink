using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ejemplo.Domain.Entities;

public partial class RegistroAuditoria
{
    public int Id { get; set; }
    public string Accion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public Administrador Administrador { get; set; } = null!;

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is RegistroAuditoria other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
