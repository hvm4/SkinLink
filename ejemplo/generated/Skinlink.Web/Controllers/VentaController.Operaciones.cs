// Fichero manual (no lo genera HARIA Web): pantallas GET de las operaciones de venta.
// Está fuera de Controllers/manual para que la regeneración no lo toque.
using ejemplo.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Skinlink.Web.Utils;

namespace Skinlink.Web.Controllers;

public partial class VentaController
{
    /// <summary>Ventas aún no publicadas: el activo no está en venta y la venta no está cerrada.</summary>
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

    /// <summary>Ventas publicadas y disponibles de otros vendedores.</summary>
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
