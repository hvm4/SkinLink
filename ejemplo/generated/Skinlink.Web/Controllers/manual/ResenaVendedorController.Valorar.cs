// Fichero manual (no lo genera HARIA Web): pantalla propia para que un usuario valore a un vendedor.
// Las acciones generadas Create (GET/POST) delegan aquí desde sus regiones protegidas cuando quien
// entra no es el administrador, para no enseñarle el formulario genérico (comprador, fecha, oculta...).
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ejemplo.Domain.Enums;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class ResenaVendedorController
{
    /// <summary>Vendedores a los que el usuario ha comprado algo (venta completada), para elegir a quién valorar.</summary>
    private async Task<List<SelectListItem>> VendedoresAValorarAsync(int compradorId, int? seleccionado)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await unitOfWork.UsuarioRepository.ReadAllAsync();
        var ventas = await unitOfWork.VentaRepository.ReadAllAsync();
        return ventas
            .Where(v => v.Comprador?.Id == compradorId && v.EstadoVenta == EstadoVenta.Completada && v.Vendedor is not null)
            .GroupBy(v => v.Vendedor.Id)
            .Select(g => new SelectListItem(
                $"{g.First().Vendedor.NombreUsuario} ({g.Count()} compra{(g.Count() == 1 ? "" : "s")})",
                g.Key.ToString(System.Globalization.CultureInfo.InvariantCulture),
                g.Key == seleccionado))
            .OrderBy(item => item.Text, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>Vista "Valorar a un vendedor" para usuarios; el administrador sigue con el formulario genérico.</summary>
    private async Task<IActionResult> VistaCrearResenaAsync(ResenaVendedorViewModel model)
    {
        if (User.EsAdministrador())
        {
            await LoadFormOptionsAsync(model);
            return View("Create", model);
        }
        ViewData["Vendedores"] = await VendedoresAValorarAsync(User.GetUsuarioId() ?? 0, model.VendedorId);
        return View("Valorar", model);
    }
}
