// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(ActivoDigitalController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(ActivoDigitalController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class ActivoDigitalController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ActivoDigitalController_BloquearParaVenta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> BloquearParaVenta(int p_oid)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_BloquearParaVenta_POST_body) ENABLED START*/

        if (!await PuedeGestionarActivoAsync(p_oid)) return Forbid();
        return await EjecutarYVolverAsync(p_oid, () => _activoDigitalService.BloquearParaVentaAsync(p_oid),
            "Activo bloqueado: ya no se puede usar en otras operaciones hasta desbloquearlo.");
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ActivoDigitalController_Desbloquear_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Desbloquear(int p_oid)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_Desbloquear_POST_body) ENABLED START*/

        if (!await PuedeGestionarActivoAsync(p_oid)) return Forbid();
        return await EjecutarYVolverAsync(p_oid, () => _activoDigitalService.DesbloquearAsync(p_oid),
            "Activo desbloqueado.");
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ActivoDigitalController_ActualizarPrecio_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ActualizarPrecio(int p_oid, Moneda p_nuevoPrecio)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_ActualizarPrecio_POST_body) ENABLED START*/

        if (!ModelState.IsValid || p_nuevoPrecio is null)
        {
            TempData["Error"] = "Indica un precio válido.";
            return RedirectToAction(nameof(Operaciones), new { id = p_oid });
        }
        return await EjecutarYVolverAsync(p_oid, () => _activoDigitalService.ActualizarPrecioAsync(p_oid, p_nuevoPrecio),
            $"Precio estimado actualizado a {p_nuevoPrecio.Valor:0.00} {p_nuevoPrecio.Divisa}.");
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ActivoDigitalController_MarcarComoFavorito_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> MarcarComoFavorito(int p_oid, int p_usuario_oid)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_MarcarComoFavorito_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _activoDigitalService.MarcarComoFavoritoAsync(p_oid, p_usuario_oid);
        return Ok(result);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ActivoDigitalController_VerificarAutenticidad_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> VerificarAutenticidad(int p_oid)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_VerificarAutenticidad_POST_body) ENABLED START*/

        try
        {
            var result = await _activoDigitalService.VerificarAutenticidadAsync(p_oid);
            return await MostrarOperacionesAsync(p_oid, autentico: result, reporte: null);
        }
        catch (Exception ex)
        {
            TempData["Error"] = MensajesOperacion.De(ex);
            return RedirectToAction(nameof(Operaciones), new { id = p_oid });
        }
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ActivoDigitalController_GenerarReporteActividad_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> GenerarReporteActividad(int p_oid)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_GenerarReporteActividad_POST_body) ENABLED START*/

        try
        {
            var result = await _activoDigitalService.GenerarReporteActividadAsync(p_oid);
            return await MostrarOperacionesAsync(p_oid, autentico: null, reporte: result);
        }
        catch (Exception ex)
        {
            TempData["Error"] = MensajesOperacion.De(ex);
            return RedirectToAction(nameof(Operaciones), new { id = p_oid });
        }
    /*PROTECTED REGION END*/
    }
}
