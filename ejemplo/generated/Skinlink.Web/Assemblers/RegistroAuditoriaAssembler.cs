// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class RegistroAuditoriaAssembler
{
    /*PROTECTED REGION ID(RegistroAuditoriaAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.RegistroAuditoriaViewModel ToViewModel(RegistroAuditoria entity) =>
        new()
        {
            Id = entity.Id,
            Accion = entity.Accion,
            Fecha = entity.Fecha,
            AdministradorId = entity.Administrador?.Id,
            AdministradorLabel = entity.Administrador is null ? null : Convert.ToString(entity.Administrador.CodigoAuditor, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(RegistroAuditoriaAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.RegistroAuditoriaViewModel model, RegistroAuditoria entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.Accion = model.Accion;
        entity.Fecha = model.Fecha;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(RegistroAuditoriaAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
