// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(MonederoController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(MonederoController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class MonederoController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_IncrementarSaldo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> IncrementarSaldo(int p_oid, Moneda p_montoAnadir)
    {
    /*PROTECTED REGION ID(MonederoController_IncrementarSaldo_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _monederoService.IncrementarSaldoAsync(p_oid, p_montoAnadir);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_DecrementarSaldo_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DecrementarSaldo(int p_oid, Moneda p_montoRestar)
    {
    /*PROTECTED REGION ID(MonederoController_DecrementarSaldo_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _monederoService.DecrementarSaldoAsync(p_oid, p_montoRestar);
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

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new ejemplo.Application.UseCases.Monedero.MonederoUseCase(unitOfWork);
        var result = await useCase.DepositarFondosAsync(p_oid, p_monto, p_referenciaPago, p_pagoConfirmado);
        return Ok(result);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_RetirarSaldoWallet_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> RetirarSaldoWallet(int p_oid, global::ejemplo.Domain.ValueObjects.Moneda p_monto, string p_destino)
    {
    /*PROTECTED REGION ID(MonederoController_RetirarSaldoWallet_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new ejemplo.Application.UseCases.Monedero.MonederoUseCase(unitOfWork);
        var result = await useCase.RetirarSaldoWalletAsync(p_oid, p_monto, p_destino);
        return Ok(result);
    /*PROTECTED REGION END*/
    }
}
