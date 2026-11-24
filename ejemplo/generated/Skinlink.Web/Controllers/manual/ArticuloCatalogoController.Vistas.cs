// Fichero manual (no lo genera HARIA Web): vista de la operación actualizarDescuentoArticulo.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skinlink.Web.Assemblers;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class ArticuloCatalogoController
{
    /// <summary>Formulario de descuento temporal (solo administradores, RF-25).</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Descuento(int? id)
    {
        var articulos = await _articuloCatalogoService.ReadAllAsync();
        var model = new ArticuloDescuentoViewModel
        {
            Articulos = articulos.OrderBy(a => a.Nombre).Select(ArticuloCatalogoAssembler.ToViewModel).ToList()
        };

        var seleccionado = model.Articulos.FirstOrDefault(a => a.Id == id) ?? model.Articulos.FirstOrDefault();
        if (seleccionado is not null)
        {
            model.ArticuloId = seleccionado.Id;
            model.Porcentaje = seleccionado.PorcentajeDescuento ?? 0;
            if (seleccionado.FechaFinDescuento is DateTime fin && fin > DateTime.Today) model.FechaFin = fin;
        }
        return View("Descuento", model);
    }
}
