// Fichero manual (no lo genera HARIA Web): pantallas GET de las operaciones de monedero.
// Está fuera de Controllers/manual para que la regeneración no lo toque.
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
    public async Task<IActionResult> RetirarSaldoWallet(int? p_oid)
    {
        var model = await CompraventaWebHelper.CargarMonederosAsync(_formUnitOfWorkFactory, User, p_oid);
        return View(model);
    }
}
