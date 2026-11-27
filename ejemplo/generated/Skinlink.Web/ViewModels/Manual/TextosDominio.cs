using System.Globalization;
using ejemplo.Domain.Enums;

namespace Skinlink.Web.ViewModels;

public static class TextosDominio
{
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-ES");

    public static string Rareza(Rareza rareza) => rareza switch
    {
        ejemplo.Domain.Enums.Rareza.Extraordinario => "Extraordinario",
        ejemplo.Domain.Enums.Rareza.Raro => "Raro",
        ejemplo.Domain.Enums.Rareza.Especial => "Especial",
        ejemplo.Domain.Enums.Rareza.PocoComun => "Poco común",
        ejemplo.Domain.Enums.Rareza.Comun => "Común",
        _ => rareza.ToString()
    };

    public static string ClaseRareza(Rareza rareza) => rareza switch
    {
        ejemplo.Domain.Enums.Rareza.Extraordinario => "text-bg-danger",
        ejemplo.Domain.Enums.Rareza.Raro => "text-bg-primary",
        ejemplo.Domain.Enums.Rareza.Especial => "text-bg-info",
        ejemplo.Domain.Enums.Rareza.PocoComun => "text-bg-warning",
        _ => "text-bg-secondary"
    };

    public static string Condicion(Condicion condicion) => condicion switch
    {
        ejemplo.Domain.Enums.Condicion.RecienFabricado => "Recién fabricado",
        ejemplo.Domain.Enums.Condicion.DesgasteMinimo => "Desgaste mínimo",
        ejemplo.Domain.Enums.Condicion.AlgoDesgastado => "Algo desgastado",
        ejemplo.Domain.Enums.Condicion.BastanteDesgastado => "Bastante desgastado",
        ejemplo.Domain.Enums.Condicion.Deplorable => "Deplorable",
        _ => condicion.ToString()
    };

    public static string EstadoIntercambio(EstadoIntercambio estado) => estado switch
    {
        ejemplo.Domain.Enums.EstadoIntercambio.Pendiente => "Pendiente",
        ejemplo.Domain.Enums.EstadoIntercambio.Aceptado => "Aceptado",
        ejemplo.Domain.Enums.EstadoIntercambio.Rechazado => "Rechazado",
        ejemplo.Domain.Enums.EstadoIntercambio.Cancelado => "Cancelado",
        _ => estado.ToString()
    };

    public static string ClaseEstadoIntercambio(EstadoIntercambio estado) => estado switch
    {
        ejemplo.Domain.Enums.EstadoIntercambio.Pendiente => "text-bg-warning",
        ejemplo.Domain.Enums.EstadoIntercambio.Aceptado => "text-bg-success",
        _ => "text-bg-secondary"
    };

    public static string Precio(double valor, string? divisa)
    {
        var simbolo = string.Equals(divisa, "EUR", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(divisa)
            ? "€"
            : divisa;
        return valor.ToString("N2", Es) + " " + simbolo;
    }
}
