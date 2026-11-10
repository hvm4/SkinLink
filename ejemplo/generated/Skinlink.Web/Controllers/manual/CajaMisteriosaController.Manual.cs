// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(CajaMisteriosaController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(CajaMisteriosaController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class CajaMisteriosaController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CajaMisteriosaController_SimularApertura_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> SimularApertura(int p_oid)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_SimularApertura_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _cajaMisteriosaService.SimularAperturaAsync(p_oid);
        return Ok(result);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CajaMisteriosaController_AbrirCajaSorpresa_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AbrirCajaSorpresa(int p_oid, int p_usuario_oid)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_AbrirCajaSorpresa_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new ejemplo.Application.UseCases.CajaMisteriosa.CajaMisteriosaUseCase(unitOfWork);
        var result = await useCase.AbrirCajaSorpresaAsync(p_oid, p_usuario_oid);
        return Ok(result);
    /*PROTECTED REGION END*/
    }
}
