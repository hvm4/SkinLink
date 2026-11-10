// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(TransaccionController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(TransaccionController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class TransaccionController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(TransaccionController_ValidarSaldoMonedero_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ValidarSaldoMonedero(int p_oid, int p_monedero_oid, Moneda p_montoRequerido)
    {
    /*PROTECTED REGION ID(TransaccionController_ValidarSaldoMonedero_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _transaccionService.ValidarSaldoMonederoAsync(p_oid, p_monedero_oid, p_montoRequerido);
        return Ok(result);
    /*PROTECTED REGION END*/
    }
}
