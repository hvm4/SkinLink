// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(VentaController_manual_usings) ENABLED START*/
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

        if (!await CompraventaWebHelper.PuedeGestionarVentaAsync(_formUnitOfWorkFactory, User, p_oid))
            return Forbid();
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Elige una venta para publicar.";
            return RedirectToAction(nameof(PublicarVenta));
        }

        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var service = new VentaService(unitOfWork);
            await service.PublicarVentaAsync(p_oid, p_activoDigital_oid);
            TempData["Ok"] = $"La venta #{p_oid} se ha publicado y el activo queda en venta.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = CompraventaWebHelper.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(PublicarVenta));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_EditarPrecio_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> EditarPrecio(int p_oid, Moneda p_nuevoPrecio)
    {
    /*PROTECTED REGION ID(VentaController_EditarPrecio_POST_body) ENABLED START*/

        if (!await CompraventaWebHelper.PuedeGestionarVentaAsync(_formUnitOfWorkFactory, User, p_oid))
            return Forbid();
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revisa el nuevo precio: importe y divisa son obligatorios.";
            return RedirectToAction(nameof(EditarPrecio), new { p_oid });
        }

        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var service = new VentaService(unitOfWork);
            await service.EditarPrecioAsync(p_oid, p_nuevoPrecio);
            TempData["Ok"] = $"Precio de la venta #{p_oid} actualizado a {p_nuevoPrecio.Valor:N2} {p_nuevoPrecio.Divisa}.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = CompraventaWebHelper.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(EditarPrecio), new { p_oid });
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_CancelarVenta_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> CancelarVenta(int p_oid)
    {
    /*PROTECTED REGION ID(VentaController_CancelarVenta_POST_body) ENABLED START*/

        if (!await CompraventaWebHelper.PuedeGestionarVentaAsync(_formUnitOfWorkFactory, User, p_oid))
            return Forbid();
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Elige una venta para cancelar.";
            return RedirectToAction(nameof(CancelarVenta));
        }

        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var service = new VentaService(unitOfWork);
            await service.CancelarVentaAsync(p_oid);
            TempData["Ok"] = $"La venta #{p_oid} se ha cancelado.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = CompraventaWebHelper.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(CancelarVenta));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_ProcesarCompraSkin_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ProcesarCompraSkin(int p_oid, int p_comprador_oid)
    {
    /*PROTECTED REGION ID(VentaController_ProcesarCompraSkin_POST_body) ENABLED START*/

        // El comprador es siempre el usuario con sesión; solo un administrador puede comprar en nombre de otro.
        if (!User.EsAdministrador())
        {
            var usuarioId = User.GetUsuarioId();
            if (usuarioId is null) return Challenge();
            if (p_comprador_oid != usuarioId.Value) return Forbid();
        }
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Elige una venta y un comprador.";
            return RedirectToAction(nameof(ProcesarCompraSkin));
        }

        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var useCase = new ejemplo.Application.UseCases.Venta.VentaUseCase(unitOfWork);
            await useCase.ProcesarCompraSkinAsync(p_oid, p_comprador_oid);
            TempData["Ok"] = $"Compra de la venta #{p_oid} completada. Revisa tu monedero y tus transacciones.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = CompraventaWebHelper.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(ProcesarCompraSkin));
    /*PROTECTED REGION END*/
    }
}
