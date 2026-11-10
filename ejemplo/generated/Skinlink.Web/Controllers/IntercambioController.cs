// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(IntercambioController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(IntercambioController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class IntercambioController : Controller
{
    /*PROTECTED REGION ID(IntercambioController_dependencies) START*/
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly IntercambioService _intercambioService;
    private readonly ILogger<IntercambioController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public IntercambioController(IntercambioService intercambioService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ejemplo.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<IntercambioController>? logger = null)
    {
        _intercambioService = intercambioService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<IntercambioController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(IntercambioController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(IntercambioController_Index_GET_body) START*/

        var entities = await _intercambioService.ReadAllAsync();
        return View(entities.Select(IntercambioAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(IntercambioController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(IntercambioController_Details_GET_body) START*/

        var entity = await _intercambioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(IntercambioAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(IntercambioController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(IntercambioController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.IntercambioViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(IntercambioController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.IntercambioViewModel model)
    {
    /*PROTECTED REGION ID(IntercambioController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new IntercambioService(unitOfWork);



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.ProponenteId is int selectedProponente &&
            !model.ProponenteOptions.Any(option => option.Value == Convert.ToString(selectedProponente, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ProponenteId", "Selecciona un registro existente de Usuario.");
        if (model.ProponenteId is null) ModelState.AddModelError("ProponenteId", "Debes seleccionar una opción.");
        if (model.ReceptorId is int selectedReceptor &&
            !model.ReceptorOptions.Any(option => option.Value == Convert.ToString(selectedReceptor, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ReceptorId", "Selecciona un registro existente de Usuario.");
        if (model.ReceptorId is null) ModelState.AddModelError("ReceptorId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Intercambio.Create");
        try
        {

            var entity = await service.NewAsync(model.Estado, model.FechaCreacion, model.ProponenteId.GetValueOrDefault(), model.ReceptorId.GetValueOrDefault());
            entity.Proponente = model.ProponenteId is int referenceProponente
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceProponente) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.Receptor = model.ReceptorId is int referenceReceptor
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceReceptor) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;

            await service.ModifierIntercambioDefaultAsync(entity);

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
    /*PROTECTED REGION ID(IntercambioController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(IntercambioController_Edit_GET_body) START*/

        var entity = await _intercambioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = IntercambioAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(IntercambioController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.IntercambioViewModel model)
    {
    /*PROTECTED REGION ID(IntercambioController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new IntercambioService(unitOfWork);
        var entity = await service.ReadOIDIntercambioDefaultAsync(id);
        if (entity is null) return NotFound();


        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.ProponenteId is int selectedProponente &&
            !model.ProponenteOptions.Any(option => option.Value == Convert.ToString(selectedProponente, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ProponenteId", "Selecciona un registro existente de Usuario.");
        if (model.ProponenteId is null) ModelState.AddModelError("ProponenteId", "Debes seleccionar una opción.");
        if (model.ReceptorId is int selectedReceptor &&
            !model.ReceptorOptions.Any(option => option.Value == Convert.ToString(selectedReceptor, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ReceptorId", "Selecciona un registro existente de Usuario.");
        if (model.ReceptorId is null) ModelState.AddModelError("ReceptorId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Intercambio.Edit");
        try
        {

            await service.ModifyAsync(id, model.Estado, model.FechaCreacion);
            entity.Proponente = model.ProponenteId is int referenceProponente
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceProponente) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.Receptor = model.ReceptorId is int referenceReceptor
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceReceptor) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;

            await service.ModifierIntercambioDefaultAsync(entity);

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
    /*PROTECTED REGION ID(IntercambioController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(IntercambioController_Delete_GET_body) START*/

        var entity = await _intercambioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(IntercambioAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(IntercambioController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(IntercambioController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.IntercambioViewModel());
        await _intercambioService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(IntercambioController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsIntercambioViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.IntercambioViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(IntercambioController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsIntercambioViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.IntercambioViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.UsuarioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.ProponenteOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.ReceptorOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
