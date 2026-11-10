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
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class UsuarioController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_SumarExperiencia_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> SumarExperiencia(int p_oid, int p_puntos)
    {
    /*PROTECTED REGION ID(UsuarioController_SumarExperiencia_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.SumarExperienciaAsync(p_oid, p_puntos);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_SuspenderCuentaUsuario_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> SuspenderCuentaUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_SuspenderCuentaUsuario_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.SuspenderCuentaUsuarioAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_VerificarUsuario_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> VerificarUsuario(int p_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_VerificarUsuario_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _usuarioService.VerificarUsuarioAsync(p_oid);
        return NoContent();
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_ReclamarRecompensaDiaria_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> ReclamarRecompensaDiaria(int p_oid, int p_caja_oid)
    {
    /*PROTECTED REGION ID(UsuarioController_ReclamarRecompensaDiaria_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return BadRequest(ModelState);
        await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
        var useCase = new ejemplo.Application.UseCases.Usuario.UsuarioUseCase(unitOfWork);
        var result = await useCase.ReclamarRecompensaDiariaAsync(p_oid, p_caja_oid);
        return Ok(result);
    /*PROTECTED REGION END*/
    }
}
