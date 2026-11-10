// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(ProbabilidadDropController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(ProbabilidadDropController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class ProbabilidadDropController : Controller
{
    /*PROTECTED REGION ID(ProbabilidadDropController_dependencies) START*/
    private readonly ProbabilidadDropService _probabilidadDropService;
    private readonly ILogger<ProbabilidadDropController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public ProbabilidadDropController(ProbabilidadDropService probabilidadDropService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<ProbabilidadDropController>? logger = null)
    {
        _probabilidadDropService = probabilidadDropService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<ProbabilidadDropController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(ProbabilidadDropController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(ProbabilidadDropController_Index_GET_body) START*/

        var entities = await _probabilidadDropService.ReadAllAsync();
        return View(entities.Select(ProbabilidadDropAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ProbabilidadDropController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(ProbabilidadDropController_Details_GET_body) START*/

        var entity = await _probabilidadDropService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ProbabilidadDropAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ProbabilidadDropController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(ProbabilidadDropController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.ProbabilidadDropViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ProbabilidadDropController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.ProbabilidadDropViewModel model)
    {
    /*PROTECTED REGION ID(ProbabilidadDropController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new ProbabilidadDropService(unitOfWork);



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.CajaId is int selectedCaja &&
            !model.CajaOptions.Any(option => option.Value == Convert.ToString(selectedCaja, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CajaId", "Selecciona un registro existente de CajaMisteriosa.");
        if (model.CajaId is null) ModelState.AddModelError("CajaId", "Debes seleccionar una opción.");
        if (model.ActivoRecompensaId is int selectedActivoRecompensa &&
            !model.ActivoRecompensaOptions.Any(option => option.Value == Convert.ToString(selectedActivoRecompensa, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ActivoRecompensaId", "Selecciona un registro existente de ActivoDigital.");
        if (model.ActivoRecompensaId is null) ModelState.AddModelError("ActivoRecompensaId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.ProbabilidadDrop.Create");
        try
        {

            var entity = await service.NewAsync(model.PorcentajeDrop, model.CajaId.GetValueOrDefault(), model.ActivoRecompensaId.GetValueOrDefault());
            entity.Caja = model.CajaId is int referenceCaja
                ? await unitOfWork.CajaMisteriosaRepository.ReadAsync(referenceCaja) ?? throw new InvalidOperationException("El registro seleccionado de CajaMisteriosa ya no existe.") : null!;
            entity.ActivoRecompensa = model.ActivoRecompensaId is int referenceActivoRecompensa
                ? await unitOfWork.ActivoDigitalRepository.ReadAsync(referenceActivoRecompensa) ?? throw new InvalidOperationException("El registro seleccionado de ActivoDigital ya no existe.") : null!;

            await service.ModifierProbabilidadDropDefaultAsync(entity);

            await unitOfWork.CommitAsync();
        }
        catch (Exception error)
        {
            await unitOfWork.RollbackAsync();
            if (error is OperationCanceledException) throw;
            _logger.LogError(error, "Could not save the record and its lines.");
            ModelState.AddModelError(string.Empty, "No se han podido guardar el registro y sus líneas.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ProbabilidadDropController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(ProbabilidadDropController_Edit_GET_body) START*/

        var entity = await _probabilidadDropService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = ProbabilidadDropAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ProbabilidadDropController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.ProbabilidadDropViewModel model)
    {
    /*PROTECTED REGION ID(ProbabilidadDropController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new ProbabilidadDropService(unitOfWork);
        var entity = await service.ReadOIDProbabilidadDropDefaultAsync(id);
        if (entity is null) return NotFound();


        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.CajaId is int selectedCaja &&
            !model.CajaOptions.Any(option => option.Value == Convert.ToString(selectedCaja, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CajaId", "Selecciona un registro existente de CajaMisteriosa.");
        if (model.CajaId is null) ModelState.AddModelError("CajaId", "Debes seleccionar una opción.");
        if (model.ActivoRecompensaId is int selectedActivoRecompensa &&
            !model.ActivoRecompensaOptions.Any(option => option.Value == Convert.ToString(selectedActivoRecompensa, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ActivoRecompensaId", "Selecciona un registro existente de ActivoDigital.");
        if (model.ActivoRecompensaId is null) ModelState.AddModelError("ActivoRecompensaId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.ProbabilidadDrop.Edit");
        try
        {

            await service.ModifyAsync(id, model.PorcentajeDrop, model.ActivoRecompensaId.GetValueOrDefault());
            entity.Caja = model.CajaId is int referenceCaja
                ? await unitOfWork.CajaMisteriosaRepository.ReadAsync(referenceCaja) ?? throw new InvalidOperationException("El registro seleccionado de CajaMisteriosa ya no existe.") : null!;
            entity.ActivoRecompensa = model.ActivoRecompensaId is int referenceActivoRecompensa
                ? await unitOfWork.ActivoDigitalRepository.ReadAsync(referenceActivoRecompensa) ?? throw new InvalidOperationException("El registro seleccionado de ActivoDigital ya no existe.") : null!;

            await service.ModifierProbabilidadDropDefaultAsync(entity);

            await unitOfWork.CommitAsync();
        }
        catch (Exception error)
        {
            await unitOfWork.RollbackAsync();
            if (error is OperationCanceledException) throw;
            _logger.LogError(error, "Could not save the record and its lines.");
            ModelState.AddModelError(string.Empty, "No se han podido guardar el registro y sus líneas.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ProbabilidadDropController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(ProbabilidadDropController_Delete_GET_body) START*/

        var entity = await _probabilidadDropService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ProbabilidadDropAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ProbabilidadDropController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(ProbabilidadDropController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.ProbabilidadDropViewModel());
        await _probabilidadDropService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(ProbabilidadDropController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsProbabilidadDropViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.ProbabilidadDropViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ProbabilidadDropController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsProbabilidadDropViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.ProbabilidadDropViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.CajaMisteriosaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.ActivoDigitalRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.CajaOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.ActivoRecompensaOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
