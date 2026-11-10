// HARIA-WEB MVC managed assembler
using ejemplo.Domain.Entities;

namespace Skinlink.Web.Assemblers;

public static class AdministradorAssembler
{
    /*PROTECTED REGION ID(AdministradorAssembler_method_ToViewModel) START*/
    public static Skinlink.Web.ViewModels.AdministradorViewModel ToViewModel(Administrador entity) =>
        new()
        {
            CodigoAuditor = entity.CodigoAuditor,
            Id = entity.Id,
            NombreUsuario = entity.NombreUsuario,
            Correo = entity.Correo,
            FechaNacimiento = entity.FechaNacimiento,
            Estado = entity.Estado,
            Nivel = entity.Nivel,
            Experiencia = entity.Experiencia,
            FechaUltimoLogin = entity.FechaUltimoLogin,
            TemaModoOscuro = entity.TemaModoOscuro,
            Idioma = entity.Idioma,
            Verificado = entity.Verificado,
            FechaUltimaRecompensa = entity.FechaUltimaRecompensa,
            MonederoId = entity.Monedero?.Id,
            MonederoLabel = entity.Monedero is null ? null : Convert.ToString(entity.Monedero.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            InventarioId = entity.Inventario?.Id,
            InventarioLabel = entity.Inventario is null ? null : Convert.ToString(entity.Inventario.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
        };
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(AdministradorAssembler_method_ApplyToEntity) START*/
    public static void ApplyToEntity(Skinlink.Web.ViewModels.AdministradorViewModel model, Administrador entity)
    {
        // Use validated input and a server-loaded entity. Persist through Application.
        // Identifiers, secrets and stored file paths are never copied from the client.
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        entity.CodigoAuditor = model.CodigoAuditor;
        entity.NombreUsuario = model.NombreUsuario;
        entity.Correo = model.Correo;
        entity.FechaNacimiento = model.FechaNacimiento;
        entity.Estado = model.Estado;
        entity.Nivel = model.Nivel;
        entity.Experiencia = model.Experiencia;
        entity.FechaUltimoLogin = model.FechaUltimoLogin;
        entity.TemaModoOscuro = model.TemaModoOscuro;
        entity.Idioma = model.Idioma;
        entity.Verificado = model.Verificado;
        entity.FechaUltimaRecompensa = model.FechaUltimaRecompensa;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(AdministradorAssembler_custom) ENABLED START*/
    // Add custom properties, validation helpers or actions here.
    /*PROTECTED REGION END*/
}
