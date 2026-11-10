// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class ResenaVendedorAssembler
{
    /*PROTECTED REGION ID(ResenaVendedorAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.ResenaVendedorViewModel ToViewModel(ResenaVendedor entity) =>
        new()
        {
            Id = entity.Id,
            Puntuacion = entity.Puntuacion,
            Comentario = entity.Comentario,
            Fecha = entity.Fecha,
            Oculta = entity.Oculta,
            CompradorId = entity.Comprador?.Id,
            CompradorLabel = entity.Comprador is null ? null : Convert.ToString(entity.Comprador.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            VendedorId = entity.Vendedor?.Id,
            VendedorLabel = entity.Vendedor is null ? null : Convert.ToString(entity.Vendedor.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ResenaVendedorAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.ResenaVendedorViewModel model, ResenaVendedor entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Puntuacion = model.Puntuacion;
        entity.Comentario = model.Comentario;
        entity.Fecha = model.Fecha;
        entity.Oculta = model.Oculta;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ResenaVendedorAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
