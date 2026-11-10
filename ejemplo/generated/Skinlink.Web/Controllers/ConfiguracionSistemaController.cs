// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(ConfiguracionSistemaController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(ConfiguracionSistemaController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class ConfiguracionSistemaController : Controller
{
    /*PROTECTED REGION ID(ConfiguracionSistemaController_dependencies) START*/
    private readonly ConfiguracionSistemaService _configuracionSistemaService;
    private readonly ILogger<ConfiguracionSistemaController> _logger;
    public ConfiguracionSistemaController(ConfiguracionSistemaService configuracionSistemaService, ILogger<ConfiguracionSistemaController>? logger = null)
    {
        _configuracionSistemaService = configuracionSistemaService;



        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<ConfiguracionSistemaController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Index_GET_body) START*/

        var entities = await _configuracionSistemaService.ReadAllAsync();
        return View(entities.Select(ConfiguracionSistemaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Details_GET_body) START*/

        var entity = await _configuracionSistemaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ConfiguracionSistemaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public IActionResult Create()
    {
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Create_GET_body) START*/

        return View(new Skinlink.Web.ViewModels.ConfiguracionSistemaViewModel());
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.ConfiguracionSistemaViewModel model)
    {
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Create_POST_body) START*/



        if (!ModelState.IsValid) return View(model);

        try
        {
            await _configuracionSistemaService.NewAsync(model.LimitePrecioBajo, model.LimitePrecioMedio, model.ComisionPrecioBajo, model.ComisionPrecioMedio, model.ComisionPrecioAlto, model.RetiroMinimo, model.RetiroMaximoDiario);
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
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Edit_GET_body) START*/

        var entity = await _configuracionSistemaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ConfiguracionSistemaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.ConfiguracionSistemaViewModel model)
    {
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Edit_POST_body) START*/


        if (!ModelState.IsValid) return View(model);

        try
        {
            await _configuracionSistemaService.ModifyAsync(id, model.LimitePrecioBajo, model.LimitePrecioMedio, model.ComisionPrecioBajo, model.ComisionPrecioMedio, model.ComisionPrecioAlto, model.RetiroMinimo, model.RetiroMaximoDiario);
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
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(ConfiguracionSistemaController_Delete_GET_body) START*/

        var entity = await _configuracionSistemaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ConfiguracionSistemaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ConfiguracionSistemaController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(ConfiguracionSistemaController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.ConfiguracionSistemaViewModel());
        await _configuracionSistemaService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }
}
