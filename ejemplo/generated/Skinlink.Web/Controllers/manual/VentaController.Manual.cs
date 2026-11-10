// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(VentaController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(VentaController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class VentaController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_PublicarVenta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> PublicarVenta(int p_oid, int p_activoDigital_oid)
    {
    /*PROTECTED REGION ID(VentaController_PublicarVenta_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _ventaService.PublicarVentaAsync(p_oid, p_activoDigital_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_EditarPrecio_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> EditarPrecio(int p_oid, Moneda p_nuevoPrecio)
    {
    /*PROTECTED REGION ID(VentaController_EditarPrecio_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _ventaService.EditarPrecioAsync(p_oid, p_nuevoPrecio);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_CancelarVenta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> CancelarVenta(int p_oid)
    {
    /*PROTECTED REGION ID(VentaController_CancelarVenta_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _ventaService.CancelarVentaAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_ProcesarCompraSkin_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ProcesarCompraSkin(int p_oid, int p_comprador_oid)
    {
    /*PROTECTED REGION ID(VentaController_ProcesarCompraSkin_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new ejemplo.Application.UseCases.Venta.VentaUseCase(unitOfWork);
        await useCase.ProcesarCompraSkinAsync(p_oid, p_comprador_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }
}
