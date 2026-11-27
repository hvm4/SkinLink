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

        // Modo prueba (RF-22): no cobra ni guarda nada; se muestra el resultado en la misma página.
        try
        {
            var result = await _cajaMisteriosaService.SimularAperturaAsync(p_oid);
            return await MostrarAperturaAsync(p_oid, result, esSimulacion: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(ex);
            return RedirectToAction(nameof(Abrir), new { id = p_oid });
        }
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CajaMisteriosaController_AbrirCajaSorpresa_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AbrirCajaSorpresa(int p_oid, int p_usuario_oid)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_AbrirCajaSorpresa_POST_body) ENABLED START*/

        // La caja se abre SIEMPRE para el usuario autenticado; p_usuario_oid del formulario se ignora.
        var usuarioId = UsuarioActualId();
        if (usuarioId is null) return Challenge();
        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var useCase = new ejemplo.Application.UseCases.CajaMisteriosa.CajaMisteriosaUseCase(unitOfWork);
            var result = await useCase.AbrirCajaSorpresaAsync(p_oid, usuarioId.Value);
            TempData["Exito"] = $"¡Has obtenido {result.Nombre} ({result.Atributos?.Rareza})! Ya está en tu inventario.";
            TempData["CajaActivoObtenidoId"] = result.Id;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(Abrir), new { id = p_oid });
    /*PROTECTED REGION END*/
    }
}
