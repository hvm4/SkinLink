// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(UsuarioController_manual_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;
using Skinlink.Web.Utils.Security;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION ID(UsuarioController_manual_class_attributes) ENABLED START*/
// La autorización de clase está en UsuarioController.cs: [Authorize(Roles = Roles.Todos)].
/*PROTECTED REGION END*/
public partial class UsuarioController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_SumarExperiencia_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> SumarExperiencia(int p_oid, int p_puntos)
    {
    /*PROTECTED REGION ID(UsuarioController_SumarExperiencia_POST_body) ENABLED START*/

        try
        {
            await _usuarioService.SumarExperienciaAsync(p_oid, p_puntos);
            TempData["Exito"] = $"Se han sumado {p_puntos} puntos de experiencia.";
            return RedirectToAction(nameof(Details), new { id = p_oid });
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(error);
            return RedirectToAction(nameof(SumarExperiencia), new { id = p_oid });
        }
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_SuspenderCuentaUsuario_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> SuspenderCuentaUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_SuspenderCuentaUsuario_POST_body) ENABLED START*/

        try
        {
            // RF-29: la operación y su registro de auditoría van en una única transacción.
            var administradorId = Skinlink.Web.Utils.Security.UsuarioClaimsExtensions.GetUsuarioId(User)
                ?? throw new global::ejemplo.Application.Exceptions.ModelException("La sesión no es válida: vuelve a iniciar sesión.");
            await using var unitOfWork = _formUnitOfWorkFactory.Create();
            await global::ejemplo.Application.Reglas.OperacionesAuditadas.SuspenderCuentaUsuarioAsync(unitOfWork, administradorId, p_oid);
            TempData["Exito"] = "La cuenta se ha suspendido. El usuario perderá el acceso la próxima vez que inicie sesión.";
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(error);
        }
        return RedirectToAction(nameof(Details), new { id = p_oid });
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_VerificarUsuario_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> VerificarUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_VerificarUsuario_POST_body) ENABLED START*/

        try
        {
            // RF-29: la operación y su registro de auditoría van en una única transacción.
            var administradorId = Skinlink.Web.Utils.Security.UsuarioClaimsExtensions.GetUsuarioId(User)
                ?? throw new global::ejemplo.Application.Exceptions.ModelException("La sesión no es válida: vuelve a iniciar sesión.");
            await using var unitOfWork = _formUnitOfWorkFactory.Create();
            await global::ejemplo.Application.Reglas.OperacionesAuditadas.VerificarUsuarioAsync(unitOfWork, administradorId, p_oid);
            TempData["Exito"] = "La identidad del usuario se ha verificado.";
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(error);
        }
        return RedirectToAction(nameof(Details), new { id = p_oid });
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_ReclamarRecompensaDiaria_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ReclamarRecompensaDiaria(int p_oid, int p_caja_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_ReclamarRecompensaDiaria_POST_body) ENABLED START*/

        // La recompensa diaria solo la reclama el propio usuario para sí mismo.
        if (User.GetUsuarioId() != p_oid) return Forbid();
        if (p_caja_oid <= 0)
        {
            TempData["Error"] = "Selecciona una caja gratuita.";
            return RedirectToAction(nameof(ReclamarRecompensaDiaria));
        }
        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var useCase = new ejemplo.Application.UseCases.Usuario.UsuarioUseCase(unitOfWork);
            var result = await useCase.ReclamarRecompensaDiariaAsync(p_oid, p_caja_oid);
            TempData["Exito"] = $"¡Recompensa diaria reclamada! Has obtenido «{result.Nombre}» ({(result.Atributos is null ? "" : Skinlink.Web.ViewModels.TextosDominio.Rareza(result.Atributos.Rareza) + ", " + Skinlink.Web.ViewModels.TextosDominio.Condicion(result.Atributos.Condicion))}) y 25 puntos de experiencia.";
            return RedirectToAction(nameof(Details), new { id = p_oid });
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(error);
            return RedirectToAction(nameof(ReclamarRecompensaDiaria));
        }
    /*PROTECTED REGION END*/
    }
}
