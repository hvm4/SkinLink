// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(InventarioPrivadoController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(InventarioPrivadoController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class InventarioPrivadoController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(InventarioPrivadoController_CambiarVisibilidadInventario_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> CambiarVisibilidadInventario(int p_oid)
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_CambiarVisibilidadInventario_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _inventarioPrivadoService.CambiarVisibilidadInventarioAsync(p_oid);
        return Ok(result);
    /*PROTECTED REGION END*/
    }
}
