using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Domain.Entities;

public partial class Venta
{
    public int Id { get; set; }
    public Moneda PrecioVenta { get; set; } = null!;
    public DateTime FechaPublicacion { get; set; }
    public EstadoVenta EstadoVenta { get; set; }
    public Usuario Vendedor { get; set; } = null!;
    public Usuario? Comprador { get; set; }
    public ActivoDigital ActivoDigital { get; set; } = null!;
    public Transaccion? TransaccionCobro { get; set; }
    public Transaccion? TransaccionPago { get; set; }

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Venta other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
