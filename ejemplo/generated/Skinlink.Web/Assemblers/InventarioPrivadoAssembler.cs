// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class InventarioPrivadoAssembler
{
    /*PROTECTED REGION ID(InventarioPrivadoAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.InventarioPrivadoViewModel ToViewModel(InventarioPrivado entity) =>
        new()
        {
            Id = entity.Id,
            ValorEstimadoTotalValor = entity.ValorEstimadoTotal?.Valor ?? default,
            ValorEstimadoTotalDivisa = entity.ValorEstimadoTotal?.Divisa ?? string.Empty,
            EsPublico = entity.EsPublico,
            UsuarioId = entity.Usuario?.Id,
            UsuarioLabel = entity.Usuario is null ? null : Convert.ToString(entity.Usuario.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            Activos = entity.Activos.Select(ActivoDigitalAssembler.ToViewModel).ToList(),
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(InventarioPrivadoAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.InventarioPrivadoViewModel model, InventarioPrivado entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.ValorEstimadoTotal ??= new global::ejemplo.Domain.ValueObjects.Moneda();
        entity.ValorEstimadoTotal!.Valor = model.ValorEstimadoTotalValor;
        entity.ValorEstimadoTotal!.Divisa = model.ValorEstimadoTotalDivisa;
        entity.EsPublico = model.EsPublico;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(InventarioPrivadoAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
