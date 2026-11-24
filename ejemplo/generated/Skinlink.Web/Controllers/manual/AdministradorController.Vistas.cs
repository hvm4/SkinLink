// Fichero manual (no lo genera HARIA Web): formulario de la operación custom actualizarEstadoKYC.
using System.Globalization;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class AdministradorController
{
    [HttpGet]
    public async Task<IActionResult> ActualizarEstadoKYC(int? usuarioId)
    {
        // Solo se ofrecen usuarios normales: el KYC de un administrador no se modifica.
        var usuarios = (await _usuarioService.ReadAllAsync())
            .Where(usuario => usuario is not Administrador)
            .OrderBy(usuario => usuario.NombreUsuario)
            .Select(usuario => new SelectListItem(
                $"{usuario.NombreUsuario} (id {usuario.Id}) · {usuario.Estado}",
                usuario.Id.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        var model = new ActualizarEstadoKYCViewModel { UsuarioId = usuarioId, Usuarios = usuarios };
        var actual = usuarioId is int id ? await _usuarioService.ReadOIDAsync(id) : null;
        if (actual is not null)
            model.NuevoEstado = actual.Estado == EstadoCuenta.Activo ? EstadoCuenta.Suspendido : EstadoCuenta.Activo;
        return View(model);
    }
}
