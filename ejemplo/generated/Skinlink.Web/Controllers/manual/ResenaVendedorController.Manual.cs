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
    /*PROTECTED REGION ID(ResenaVendedorController_ModerarResena_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ModerarResena(int p_oid, int p_admin_oid)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_ModerarResena_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _resenaVendedorService.ModerarResenaAsync(p_oid, p_admin_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ResenaVendedorController_OcultarResena_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> OcultarResena(int p_oid)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_OcultarResena_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _resenaVendedorService.OcultarResenaAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
