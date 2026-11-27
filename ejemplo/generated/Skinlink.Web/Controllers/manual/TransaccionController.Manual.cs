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
    /*PROTECTED REGION ID(TransaccionController_ValidarSaldoMonedero_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ValidarSaldoMonedero(int p_oid, int p_monedero_oid, Moneda p_montoRequerido)
    {
    /*PROTECTED REGION ID(TransaccionController_ValidarSaldoMonedero_POST_body) ENABLED START*/
        // Consulta sin efectos: indica si el monedero cubre el importe (misma divisa) y vuelve al formulario.
        // p_oid (Transaccion) no interviene en la comprobación; el formulario envía 0.
        if (!ModelState.IsValid || p_montoRequerido is null)
        {
            TempData["Error"] = "Indica un monedero, un importe y una divisa válidos.";
            return RedirectToAction(nameof(ValidarSaldoMonedero), new { p_monedero_oid });
        }
        try
        {
            var cubre = await _transaccionService.ValidarSaldoMonederoAsync(p_oid, p_monedero_oid, p_montoRequerido);
            var importe = $"{p_montoRequerido.Valor:N2} {p_montoRequerido.Divisa}";
            if (cubre)
                TempData["Exito"] = $"El monedero #{p_monedero_oid} tiene saldo suficiente para {importe}.";
            else
                TempData["Error"] = $"El monedero #{p_monedero_oid} NO tiene saldo suficiente (o en esa divisa) para {importe}.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(ValidarSaldoMonedero), new { p_monedero_oid });
    /*PROTECTED REGION END*/
    }
}
