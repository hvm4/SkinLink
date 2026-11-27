// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(ResenaVendedorController_manual_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION ID(ResenaVendedorController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class ResenaVendedorController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ResenaVendedorController_ModerarResena_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ModerarResena(int p_oid, int p_admin_oid)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_ModerarResena_POST_body) ENABLED START*/
        // Miembro 4: el administrador que modera es siempre el de la sesión; p_admin_oid se ignora.
        if (Skinlink.Web.Utils.Security.UsuarioClaimsExtensions.GetUsuarioId(User) is not int adminId) return Challenge();
        try
        {
            await _resenaVendedorService.ModerarResenaAsync(p_oid, adminId);
            TempData["Exito"] = $"Reseña #{p_oid} moderada. El cambio queda en el registro de auditoría.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(Moderacion));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ResenaVendedorController_OcultarResena_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> OcultarResena(int p_oid)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_OcultarResena_POST_body) ENABLED START*/
        // Miembro 4: solo el vendedor reseñado (o un administrador) puede ocultar la reseña de su perfil.
        return await OcultarResenaDeMiPerfilAsync(p_oid);
    /*PROTECTED REGION END*/
    }
}
