using System.ComponentModel.DataAnnotations;
using ejemplo.Domain.Enums;

namespace Skinlink.Web.ViewModels;

public sealed class TransaccionFiltroTipoViewModel
{
    [Display(Name = "Tipo de transacción")]
    public TipoTransaccion Tipo { get; set; } = TipoTransaccion.Deposito;

    public bool Buscado { get; set; }

    public bool SoloMisMovimientos { get; set; }

    public List<TransaccionViewModel> Resultados { get; set; } = new();

    public double Total => Resultados.Sum(t => t.MontoValor);
    public string Divisa => Resultados.FirstOrDefault()?.MontoDivisa ?? "EUR";
}
