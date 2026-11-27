using Microsoft.AspNetCore.Mvc;
using Skinlink.Web.Utils;

namespace Skinlink.Web.Controllers;

public partial class MonederoController
{
    [HttpGet]
    public async Task<IActionResult> DepositarFondos(int? p_oid)
    {
        var model = await CompraventaWebHelper.CargarMonederosAsync(_formUnitOfWorkFactory, User, p_oid);
        return View(model);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    public async Task<IActionResult> AjustarSaldo(int? p_oid)
    {
        var model = await CompraventaWebHelper.CargarMonederosAsync(_formUnitOfWorkFactory, User, p_oid);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> RetirarSaldoWallet(int? p_oid)
    {
        var model = await CompraventaWebHelper.CargarMonederosAsync(_formUnitOfWorkFactory, User, p_oid);
        return View(model);
    }
}
