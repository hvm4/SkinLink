using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Domain.Entities;

public partial class ArticuloCatalogo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Coleccion { get; set; } = string.Empty;
    public Rareza Rareza { get; set; }
    public Moneda PrecioReferencia { get; set; } = null!;
    public bool Habilitado { get; set; }
    public double? PorcentajeDescuento { get; set; }
    public DateTime? FechaFinDescuento { get; set; }

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ArticuloCatalogo other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
