using ejemplo.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Skinlink.Web.Utils;

namespace Skinlink.Web.Controllers;

public partial class VentaController
{
    [HttpGet]
    public async Task<IActionResult> PublicarVenta(int? p_oid)
    {
        var model = await CompraventaWebHelper.CargarVentasAsync(
            _formUnitOfWorkFactory, User, ModoVentas.Gestionar,
            v => v.EstadoVenta is not (EstadoVenta.Completada or EstadoVenta.Cancelada)
                 && v.ActivoDigital is not null && !v.ActivoDigital.EnVenta,
            p_oid);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditarPrecio(int? p_oid)
    {
        var model = await CompraventaWebHelper.CargarVentasAsync(
            _formUnitOfWorkFactory, User, ModoVentas.Gestionar,
            v => v.EstadoVenta == EstadoVenta.Disponible, p_oid);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CancelarVenta(int? p_oid)
    {
        var model = await CompraventaWebHelper.CargarVentasAsync(
            _formUnitOfWorkFactory, User, ModoVentas.Gestionar,
            v => v.EstadoVenta is EstadoVenta.Disponible or EstadoVenta.EnProceso, p_oid);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ProcesarCompraSkin(int? p_oid)
    {
        var model = await CompraventaWebHelper.CargarVentasAsync(
            _formUnitOfWorkFactory, User, ModoVentas.Comprar,
            v => v.EstadoVenta == EstadoVenta.Disponible && v.ActivoDigital is not null && v.ActivoDigital.EnVenta,
            p_oid);
        return View(model);
    }
}
