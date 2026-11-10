// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class MonederoAssembler
{
    /*PROTECTED REGION ID(MonederoAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.MonederoViewModel ToViewModel(Monedero entity) =>
        new()
        {
            Id = entity.Id,
            SaldoDisponibleValor = entity.SaldoDisponible?.Valor ?? default,
            SaldoDisponibleDivisa = entity.SaldoDisponible?.Divisa ?? string.Empty,
            SaldoPendienteValor = entity.SaldoPendiente?.Valor ?? default,
            SaldoPendienteDivisa = entity.SaldoPendiente?.Divisa ?? string.Empty,
            UsuarioId = entity.Usuario?.Id,
            UsuarioLabel = entity.Usuario is null ? null : Convert.ToString(entity.Usuario.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(MonederoAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.MonederoViewModel model, Monedero entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.SaldoDisponible ??= new global::ejemplo.Domain.ValueObjects.Moneda();
        entity.SaldoDisponible!.Valor = model.SaldoDisponibleValor;
        entity.SaldoDisponible!.Divisa = model.SaldoDisponibleDivisa;
        entity.SaldoPendiente ??= new global::ejemplo.Domain.ValueObjects.Moneda();
        entity.SaldoPendiente!.Valor = model.SaldoPendienteValor;
        entity.SaldoPendiente!.Divisa = model.SaldoPendienteDivisa;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(MonederoAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
