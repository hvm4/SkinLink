// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(FavoritoController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(FavoritoController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class FavoritoController : Controller
{
    /*PROTECTED REGION ID(FavoritoController_dependencies) START*/
    private readonly FavoritoService _favoritoService;
    private readonly ILogger<FavoritoController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public FavoritoController(FavoritoService favoritoService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<FavoritoController>? logger = null)
    {
        _favoritoService = favoritoService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<FavoritoController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(FavoritoController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(FavoritoController_Index_GET_body) START*/

        var entities = await _favoritoService.ReadAllAsync();
        return View(entities.Select(FavoritoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(FavoritoController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(FavoritoController_Details_GET_body) START*/

        var entity = await _favoritoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(FavoritoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(FavoritoController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(FavoritoController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.FavoritoViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(FavoritoController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.FavoritoViewModel model)
    {
    /*PROTECTED REGION ID(FavoritoController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new FavoritoService(unitOfWork);



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");
        if (model.ActivoDigitalId is int selectedActivoDigital &&
            !model.ActivoDigitalOptions.Any(option => option.Value == Convert.ToString(selectedActivoDigital, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ActivoDigitalId", "Selecciona un registro existente de ActivoDigital.");
        if (model.ActivoDigitalId is null) ModelState.AddModelError("ActivoDigitalId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Favorito.Create");
        try
        {

            var entity = await service.NewAsync(model.FechaMarcado, model.UsuarioId.GetValueOrDefault(), model.ActivoDigitalId.GetValueOrDefault());
            entity.Usuario = model.UsuarioId is int referenceUsuario
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceUsuario) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.ActivoDigital = model.ActivoDigitalId is int referenceActivoDigital
                ? await unitOfWork.ActivoDigitalRepository.ReadAsync(referenceActivoDigital) ?? throw new InvalidOperationException("El registro seleccionado de ActivoDigital ya no existe.") : null!;

            await service.ModifierFavoritoDefaultAsync(entity);

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
    /*PROTECTED REGION ID(FavoritoController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(FavoritoController_Edit_GET_body) START*/

        var entity = await _favoritoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = FavoritoAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(FavoritoController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.FavoritoViewModel model)
    {
    /*PROTECTED REGION ID(FavoritoController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new FavoritoService(unitOfWork);
        var entity = await service.ReadOIDFavoritoDefaultAsync(id);
        if (entity is null) return NotFound();


        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");
        if (model.ActivoDigitalId is int selectedActivoDigital &&
            !model.ActivoDigitalOptions.Any(option => option.Value == Convert.ToString(selectedActivoDigital, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ActivoDigitalId", "Selecciona un registro existente de ActivoDigital.");
        if (model.ActivoDigitalId is null) ModelState.AddModelError("ActivoDigitalId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Favorito.Edit");
        try
        {

            await service.ModifyAsync(id, model.FechaMarcado);
            entity.Usuario = model.UsuarioId is int referenceUsuario
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceUsuario) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.ActivoDigital = model.ActivoDigitalId is int referenceActivoDigital
                ? await unitOfWork.ActivoDigitalRepository.ReadAsync(referenceActivoDigital) ?? throw new InvalidOperationException("El registro seleccionado de ActivoDigital ya no existe.") : null!;

            await service.ModifierFavoritoDefaultAsync(entity);

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
    /*PROTECTED REGION ID(FavoritoController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(FavoritoController_Delete_GET_body) START*/

        var entity = await _favoritoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(FavoritoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(FavoritoController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(FavoritoController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.FavoritoViewModel());
        await _favoritoService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(FavoritoController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsFavoritoViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.FavoritoViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(FavoritoController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsFavoritoViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.FavoritoViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.UsuarioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.ActivoDigitalRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.UsuarioOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.ActivoDigitalOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
