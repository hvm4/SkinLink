using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ejemplo.Domain.Entities;

public partial class ConfiguracionSistema
{
    public int Id { get; set; }
    public double LimitePrecioBajo { get; set; }
    public double LimitePrecioMedio { get; set; }
    public double ComisionPrecioBajo { get; set; }
    public double ComisionPrecioMedio { get; set; }
    public double ComisionPrecioAlto { get; set; }
    public double RetiroMinimo { get; set; }
    public double RetiroMaximoDiario { get; set; }

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ConfiguracionSistema other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
