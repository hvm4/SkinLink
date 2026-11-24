// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(MonederoController_manual_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using Skinlink.Web.Utils;
using Skinlink.Web.Utils.Security;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION ID(MonederoController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class MonederoController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_IncrementarSaldo_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> IncrementarSaldo(int p_oid, Moneda p_montoAnadir)
    {
    /*PROTECTED REGION ID(MonederoController_IncrementarSaldo_POST_body) ENABLED START*/

        // Operación interna: modifica el saldo sin pasar por una transacción, solo para administradores.
        // Los usuarios normales usan DepositarFondos.
        if (!User.EsAdministrador()) return Forbid();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _monederoService.IncrementarSaldoAsync(p_oid, p_montoAnadir);
        }
        catch (ejemplo.Application.Exceptions.ModelException ex)
        {
            return BadRequest(CompraventaWebHelper.MensajeDeError(ex));
        }
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_DecrementarSaldo_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DecrementarSaldo(int p_oid, Moneda p_montoRestar)
    {
    /*PROTECTED REGION ID(MonederoController_DecrementarSaldo_POST_body) ENABLED START*/

        if (!User.EsAdministrador()) return Forbid();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            await _monederoService.DecrementarSaldoAsync(p_oid, p_montoRestar);
        }
        catch (ejemplo.Application.Exceptions.ModelException ex)
        {
            return BadRequest(CompraventaWebHelper.MensajeDeError(ex));
        }
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_DepositarFondos_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DepositarFondos(int p_oid, global::ejemplo.Domain.ValueObjects.Moneda p_monto, string p_referenciaPago, bool p_pagoConfirmado)
    {
    /*PROTECTED REGION ID(MonederoController_DepositarFondos_POST_body) ENABLED START*/

        if (!await CompraventaWebHelper.PuedeUsarMonederoAsync(_formUnitOfWorkFactory, User, p_oid))
            return Forbid();
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revisa los datos del formulario: importe, divisa y referencia de pago son obligatorios.";
            return RedirectToAction(nameof(DepositarFondos), new { p_oid });
        }

        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var useCase = new ejemplo.Application.UseCases.Monedero.MonederoUseCase(unitOfWork);
            await useCase.DepositarFondosAsync(p_oid, p_monto, p_referenciaPago, p_pagoConfirmado);
            TempData["Ok"] = $"Depósito realizado: {p_monto.Valor:N2} {p_monto.Divisa} (referencia {p_referenciaPago}).";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = CompraventaWebHelper.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(DepositarFondos), new { p_oid });
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_RetirarSaldoWallet_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RetirarSaldoWallet(int p_oid, global::ejemplo.Domain.ValueObjects.Moneda p_monto, string p_destino)
    {
    /*PROTECTED REGION ID(MonederoController_RetirarSaldoWallet_POST_body) ENABLED START*/

        if (!await CompraventaWebHelper.PuedeUsarMonederoAsync(_formUnitOfWorkFactory, User, p_oid))
            return Forbid();
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revisa los datos del formulario: importe, divisa y destino son obligatorios.";
            return RedirectToAction(nameof(RetirarSaldoWallet), new { p_oid });
        }

        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var useCase = new ejemplo.Application.UseCases.Monedero.MonederoUseCase(unitOfWork);
            await useCase.RetirarSaldoWalletAsync(p_oid, p_monto, p_destino);
            TempData["Ok"] = $"Retirada realizada: {p_monto.Valor:N2} {p_monto.Divisa} hacia {p_destino}.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = CompraventaWebHelper.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(RetirarSaldoWallet), new { p_oid });
    /*PROTECTED REGION END*/
    }
}
