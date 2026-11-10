// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(ArticuloCatalogoController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(ArticuloCatalogoController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class ArticuloCatalogoController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ArticuloCatalogoController_ActualizarDescuentoArticulo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ActualizarDescuentoArticulo(int p_oid, double p_porcentaje, DateTime p_fechaFin)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_ActualizarDescuentoArticulo_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _articuloCatalogoService.ActualizarDescuentoArticuloAsync(p_oid, p_porcentaje, p_fechaFin);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
