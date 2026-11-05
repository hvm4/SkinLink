using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Domain.Entities;

public partial class Transaccion
{
    public int Id { get; set; }
    public TipoTransaccion Tipo { get; set; }
    public Moneda Monto { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public Monedero Monedero { get; set; } = null!;
    public Venta? VentaCobro { get; set; }
    public Venta? VentaPago { get; set; }

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Transaccion other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
