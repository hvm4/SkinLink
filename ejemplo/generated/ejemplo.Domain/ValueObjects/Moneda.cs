using System.Collections.Generic;

namespace ejemplo.Domain.ValueObjects;

public partial class Moneda
{
    public double Valor { get; set; }
    public string Divisa { get; set; } = string.Empty;

    public override bool Equals(object? obj) => obj is Moneda other &&
        EqualityComparer<double>.Default.Equals(Valor, other.Valor) &&
        EqualityComparer<string>.Default.Equals(Divisa, other.Divisa);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Valor);
        hash.Add(Divisa);
        return hash.ToHashCode();
    }
}
