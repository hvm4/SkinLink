using System.Collections.Generic;
using ejemplo.Domain.Enums;

namespace ejemplo.Domain.ValueObjects;

public partial class AtributosSkin
{
    public Rareza Rareza { get; set; }
    public Condicion Condicion { get; set; }
    public double FloatVal { get; set; }
    public int Patron { get; set; }

    public override bool Equals(object? obj) => obj is AtributosSkin other &&
        EqualityComparer<Rareza>.Default.Equals(Rareza, other.Rareza) &&
        EqualityComparer<Condicion>.Default.Equals(Condicion, other.Condicion) &&
        EqualityComparer<double>.Default.Equals(FloatVal, other.FloatVal) &&
        EqualityComparer<int>.Default.Equals(Patron, other.Patron);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Rareza);
        hash.Add(Condicion);
        hash.Add(FloatVal);
        hash.Add(Patron);
        return hash.ToHashCode();
    }
}
