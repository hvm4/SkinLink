// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(AdministradorController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(AdministradorController_manual_class_attributes) ENABLED START*/
// La autorización de clase está en AdministradorController.cs: [Authorize(Roles = Roles.Administrador)].
/*PROTECTED REGION END*/
public partial class AdministradorController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AdministradorController_ActualizarEstadoKYC_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ActualizarEstadoKYC(int p_oid, int p_usuario_oid, EstadoCuenta p_nuevoEstado)
    {
    /*PROTECTED REGION ID(AdministradorController_ActualizarEstadoKYC_POST_body) ENABLED START*/

        // El administrador que actúa es siempre el de la sesión (no se acepta otro p_oid del formulario).
        var administradorId = User.GetUsuarioId();
        if (administradorId is null) return Challenge();
        try
        {
            await _administradorService.ActualizarEstadoKYCAsync(administradorId.Value, p_usuario_oid, p_nuevoEstado);
            TempData["Exito"] = $"Estado KYC actualizado a {p_nuevoEstado}. La acción ha quedado en el registro de auditoría.";
            return RedirectToAction("Details", "Usuario", new { id = p_usuario_oid });
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["Error"] = MensajesOperacion.De(error);
            return RedirectToAction(nameof(ActualizarEstadoKYC), new { usuarioId = p_usuario_oid });
        }
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AdministradorController_SumarExperiencia_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> SumarExperiencia(int p_oid, int p_puntos)
    {
    /*PROTECTED REGION ID(AdministradorController_SumarExperiencia_POST_body) ENABLED START*/

        // Operación heredada de Usuario (herencia Usuario <- Administrador).
        try
        {
            await _usuarioService.SumarExperienciaAsync(p_oid, p_puntos);
            TempData["Exito"] = $"Se han sumado {p_puntos} puntos de experiencia.";
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["Error"] = MensajesOperacion.De(error);
        }
        return RedirectToAction("Details", "Usuario", new { id = p_oid });
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AdministradorController_SuspenderCuentaUsuario_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> SuspenderCuentaUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(AdministradorController_SuspenderCuentaUsuario_POST_body) ENABLED START*/

        // Operación heredada de Usuario (herencia Usuario <- Administrador).
        try
        {
            // RF-29: la operación y su registro de auditoría van en una única transacción.
            var administradorId = Skinlink.Web.Utils.Security.UsuarioClaimsExtensions.GetUsuarioId(User)
                ?? throw new InvalidOperationException("La sesión no es válida: vuelve a iniciar sesión.");
            await using var unitOfWork = _formUnitOfWorkFactory.Create();
            await global::ejemplo.Application.Reglas.OperacionesAuditadas.SuspenderCuentaUsuarioAsync(unitOfWork, administradorId, p_oid);
            TempData["Exito"] = $"La cuenta se ha suspendido.";
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["Error"] = MensajesOperacion.De(error);
        }
        return RedirectToAction("Details", "Usuario", new { id = p_oid });
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AdministradorController_VerificarUsuario_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> VerificarUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(AdministradorController_VerificarUsuario_POST_body) ENABLED START*/

        // Operación heredada de Usuario (herencia Usuario <- Administrador).
        try
        {
            // RF-29: la operación y su registro de auditoría van en una única transacción.
            var administradorId = Skinlink.Web.Utils.Security.UsuarioClaimsExtensions.GetUsuarioId(User)
                ?? throw new InvalidOperationException("La sesión no es válida: vuelve a iniciar sesión.");
            await using var unitOfWork = _formUnitOfWorkFactory.Create();
            await global::ejemplo.Application.Reglas.OperacionesAuditadas.VerificarUsuarioAsync(unitOfWork, administradorId, p_oid);
            TempData["Exito"] = $"La identidad se ha verificado.";
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["Error"] = MensajesOperacion.De(error);
        }
        return RedirectToAction("Details", "Usuario", new { id = p_oid });
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AdministradorController_ReclamarRecompensaDiaria_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ReclamarRecompensaDiaria(int p_oid, int p_caja_oid)
    {
    /*PROTECTED REGION ID(AdministradorController_ReclamarRecompensaDiaria_POST_body) ENABLED START*/

        // Un administrador también es Usuario (herencia): reclama su propia recompensa.
        if (User.GetUsuarioId() != p_oid) return Forbid();
        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var useCase = new ejemplo.Application.UseCases.Usuario.UsuarioUseCase(unitOfWork);
            var result = await useCase.ReclamarRecompensaDiariaAsync(p_oid, p_caja_oid);
            TempData["Exito"] = $"¡Recompensa diaria reclamada! Has obtenido «{result.Nombre}».";
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["Error"] = MensajesOperacion.De(error);
        }
        return RedirectToAction("Details", "Usuario", new { id = p_oid });
    /*PROTECTED REGION END*/
    }
}
