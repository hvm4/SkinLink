// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(RegistroAuditoriaController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(RegistroAuditoriaController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class RegistroAuditoriaController : Controller
{
    /*PROTECTED REGION ID(RegistroAuditoriaController_dependencies) START*/
    private readonly RegistroAuditoriaService _registroAuditoriaService;
    private readonly ILogger<RegistroAuditoriaController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public RegistroAuditoriaController(RegistroAuditoriaService registroAuditoriaService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<RegistroAuditoriaController>? logger = null)
    {
        _registroAuditoriaService = registroAuditoriaService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<RegistroAuditoriaController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(RegistroAuditoriaController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(RegistroAuditoriaController_Index_GET_body) START*/

        var entities = await _registroAuditoriaService.ReadAllAsync();
        return View(entities.Select(RegistroAuditoriaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(RegistroAuditoriaController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(RegistroAuditoriaController_Details_GET_body) START*/

        var entity = await _registroAuditoriaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(RegistroAuditoriaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(RegistroAuditoriaController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(RegistroAuditoriaController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.RegistroAuditoriaViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(RegistroAuditoriaController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.RegistroAuditoriaViewModel model)
    {
    /*PROTECTED REGION ID(RegistroAuditoriaController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new RegistroAuditoriaService(unitOfWork);



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.AdministradorId is int selectedAdministrador &&
            !model.AdministradorOptions.Any(option => option.Value == Convert.ToString(selectedAdministrador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("AdministradorId", "Selecciona un registro existente de Administrador.");
        if (model.AdministradorId is null) ModelState.AddModelError("AdministradorId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.RegistroAuditoria.Create");
        try
        {

            var entity = await service.NewAsync(model.Accion, model.Fecha, model.AdministradorId.GetValueOrDefault());
            entity.Administrador = model.AdministradorId is int referenceAdministrador
                ? await unitOfWork.AdministradorRepository.ReadAsync(referenceAdministrador) ?? throw new InvalidOperationException("El registro seleccionado de Administrador ya no existe.") : null!;

            await service.ModifierRegistroAuditoriaDefaultAsync(entity);

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
    /*PROTECTED REGION ID(RegistroAuditoriaController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(RegistroAuditoriaController_Delete_GET_body) START*/

        var entity = await _registroAuditoriaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(RegistroAuditoriaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(RegistroAuditoriaController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(RegistroAuditoriaController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.RegistroAuditoriaViewModel());
        await _registroAuditoriaService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(RegistroAuditoriaController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsRegistroAuditoriaViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.RegistroAuditoriaViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(RegistroAuditoriaController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsRegistroAuditoriaViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.RegistroAuditoriaViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.AdministradorRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.CodigoAuditor, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.AdministradorOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
