// HARIA-WEB MVC managed manual-controller
/*PROTECTED REGION ID(IntercambioController_manual_usings) ENABLED START*/
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
/*PROTECTED REGION ID(IntercambioController_manual_class_attributes) ENABLED START*/
// Class-level authorization applies to both partial files.
// [Authorize(Roles = "Admin")]
/*PROTECTED REGION END*/
public partial class IntercambioController
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(IntercambioController_IniciarIntercambio_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> IniciarIntercambio(int p_proponente_oid, int p_receptor_oid, IReadOnlyList<int> p_activosOfrecidos_oids, IReadOnlyList<int> p_activosSolicitados_oids)
    {
    /*PROTECTED REGION ID(IntercambioController_IniciarIntercambio_POST_body) ENABLED START*/

        // El proponente es siempre el usuario de la sesión; p_proponente_oid se ignora.
        return await IniciarIntercambioDesdeSesionAsync(p_receptor_oid, p_activosOfrecidos_oids, p_activosSolicitados_oids);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(IntercambioController_AceptarIntercambio_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> AceptarIntercambio(int p_oid, int p_receptor_oid)
    {
    /*PROTECTED REGION ID(IntercambioController_AceptarIntercambio_POST_body) ENABLED START*/

        // Solo acepta el receptor, que es el usuario de la sesión; p_receptor_oid se ignora.
        return await ResolverIntercambioAsync(p_oid, aceptar: true);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(IntercambioController_CancelarIntercambio_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> CancelarIntercambio(int p_oid, int p_usuario_oid)
    {
    /*PROTECTED REGION ID(IntercambioController_CancelarIntercambio_POST_body) ENABLED START*/

        // Cancela el usuario de la sesión (proponente o receptor); p_usuario_oid se ignora.
        return await ResolverIntercambioAsync(p_oid, aceptar: false);
    /*PROTECTED REGION END*/
    }
}
