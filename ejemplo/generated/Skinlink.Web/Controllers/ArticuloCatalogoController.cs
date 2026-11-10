// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(ArticuloCatalogoController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(ArticuloCatalogoController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class ArticuloCatalogoController : Controller
{
    /*PROTECTED REGION ID(ArticuloCatalogoController_dependencies) START*/
    private readonly ArticuloCatalogoService _articuloCatalogoService;
    private readonly ILogger<ArticuloCatalogoController> _logger;
    public ArticuloCatalogoController(ArticuloCatalogoService articuloCatalogoService, ILogger<ArticuloCatalogoController>? logger = null)
    {
        _articuloCatalogoService = articuloCatalogoService;



        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<ArticuloCatalogoController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Index_GET_body) START*/

        var entities = await _articuloCatalogoService.ReadAllAsync();
        return View(entities.Select(ArticuloCatalogoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Details_GET_body) START*/

        var entity = await _articuloCatalogoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ArticuloCatalogoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public IActionResult Create()
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Create_GET_body) START*/

        return View(new Skinlink.Web.ViewModels.ArticuloCatalogoViewModel());
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.ArticuloCatalogoViewModel model)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Create_POST_body) START*/



        if (!ModelState.IsValid) return View(model);

        try
        {
            await _articuloCatalogoService.NewAsync(model.Nombre, model.Coleccion, model.Rareza, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioReferenciaValor, Divisa = model.PrecioReferenciaDivisa }, model.Habilitado, model.PorcentajeDescuento ?? 0, model.FechaFinDescuento ?? DateTime.MinValue);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "Could not create the record.");
            ModelState.AddModelError(string.Empty, "No se ha podido guardar el registro. Inténtalo de nuevo.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Edit_GET_body) START*/

        var entity = await _articuloCatalogoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ArticuloCatalogoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.ArticuloCatalogoViewModel model)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Edit_POST_body) START*/


        if (!ModelState.IsValid) return View(model);

        try
        {
            await _articuloCatalogoService.ModifyAsync(id, model.Nombre, model.Coleccion, model.Rareza, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioReferenciaValor, Divisa = model.PrecioReferenciaDivisa }, model.Habilitado, model.PorcentajeDescuento ?? 0, model.FechaFinDescuento ?? DateTime.MinValue);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "Could not update the record.");
            ModelState.AddModelError(string.Empty, "No se ha podido guardar el registro. Inténtalo de nuevo.");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Delete_GET_body) START*/

        var entity = await _articuloCatalogoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ArticuloCatalogoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ArticuloCatalogoController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.ArticuloCatalogoViewModel());
        await _articuloCatalogoService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }
}
