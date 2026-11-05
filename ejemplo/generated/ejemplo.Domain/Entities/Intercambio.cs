using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ejemplo.Domain.Enums;

namespace ejemplo.Domain.Entities;

public partial class Intercambio
{
    public int Id { get; set; }
    public EstadoIntercambio Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public Usuario Proponente { get; set; } = null!;
    public Usuario Receptor { get; set; } = null!;
    public IList<ActivoDigital> ActivosOfrecidos { get; set; } = new List<ActivoDigital>();
    public IList<ActivoDigital> ActivosSolicitados { get; set; } = new List<ActivoDigital>();

    private bool HasDefaultIdentity() => EqualityComparer<int>.Default.Equals(Id, default);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Intercambio other && !HasDefaultIdentity() && !other.HasDefaultIdentity() && EqualityComparer<int>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => HasDefaultIdentity() ? RuntimeHelpers.GetHashCode(this) : EqualityComparer<int>.Default.GetHashCode(Id);
}
