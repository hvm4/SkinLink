using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skinlink.Web.Utils;

namespace Skinlink.Web.Controllers;

public partial class TransaccionController
{
    [HttpGet]
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    public async Task<IActionResult> ValidarSaldoMonedero(int? p_monedero_oid)
    {
        var model = await CompraventaWebHelper.CargarMonederosAsync(_formUnitOfWorkFactory, User, p_monedero_oid);
        return View(model);
    }
}
