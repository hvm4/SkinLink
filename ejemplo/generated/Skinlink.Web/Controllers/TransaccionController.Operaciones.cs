// Fichero manual (no lo genera HARIA Web): pantalla GET de validarSaldoMonedero.
// Está fuera de Controllers/manual, como las de Monedero y Venta, para que la regeneración no lo toque.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skinlink.Web.Utils;

namespace Skinlink.Web.Controllers;

public partial class TransaccionController
{
    /// <summary>Comprueba si un monedero cubre un importe (validarSaldoMonedero). Solo administradores.</summary>
    [HttpGet]
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    public async Task<IActionResult> ValidarSaldoMonedero(int? p_monedero_oid)
    {
        var model = await CompraventaWebHelper.CargarMonederosAsync(_formUnitOfWorkFactory, User, p_monedero_oid);
        return View(model);
    }
}
