// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(InventarioPrivadoController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(InventarioPrivadoController_class_attributes) ENABLED START*/
// Seguridad (Miembro 1): cualquier cuenta activa; las cuentas suspendidas no tienen rol.
// Las acciones con [AllowAnonymous] siguen siendo públicas.
[Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Todos)]
/*PROTECTED REGION END*/
public partial class InventarioPrivadoController : Controller
{
    /*PROTECTED REGION ID(InventarioPrivadoController_dependencies) START*/
    private readonly InventarioPrivadoService _inventarioPrivadoService;
    private readonly ILogger<InventarioPrivadoController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public InventarioPrivadoController(InventarioPrivadoService inventarioPrivadoService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<InventarioPrivadoController>? logger = null)
    {
        _inventarioPrivadoService = inventarioPrivadoService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<InventarioPrivadoController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(InventarioPrivadoController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_Index_GET_body) START*/

        var entities = await _inventarioPrivadoService.ReadAllAsync();
        return View(entities.Select(InventarioPrivadoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(InventarioPrivadoController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_Details_GET_body) START*/

        var entity = await _inventarioPrivadoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(InventarioPrivadoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(InventarioPrivadoController_Create_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.InventarioPrivadoViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(InventarioPrivadoController_Create_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.InventarioPrivadoViewModel model, IReadOnlyList<ActivoDigital> p_activos)
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::ejemplo.Application.Services.InventarioPrivadoService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.InventarioPrivado.Create");


            var entity = await service.NewAsync(new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.ValorEstimadoTotalValor, Divisa = model.ValorEstimadoTotalDivisa }, model.EsPublico, model.UsuarioId.GetValueOrDefault(), p_activos);

            await service.ApplyInventarioPrivadoRelationsAsync(entity, p_usuario_id: model.UsuarioId);

            await service.ModifierInventarioPrivadoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(InventarioPrivadoController_Edit_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_Edit_GET_body) START*/

        var entity = await _inventarioPrivadoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = InventarioPrivadoAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(InventarioPrivadoController_Edit_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.InventarioPrivadoViewModel model)
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::ejemplo.Application.Services.InventarioPrivadoService(unitOfWork);

        var entity = await service.ReadOIDInventarioPrivadoDefaultAsync(id);
        if (entity is null) return NotFound();



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.InventarioPrivado.Edit");


            await service.ModifyAsync(id, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.ValorEstimadoTotalValor, Divisa = model.ValorEstimadoTotalDivisa }, model.EsPublico);
            entity = await service.ReadOIDInventarioPrivadoDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyInventarioPrivadoRelationsAsync(entity, p_usuario_id: model.UsuarioId);

            await service.ModifierInventarioPrivadoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(InventarioPrivadoController_Delete_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_Delete_GET_body) START*/

        var entity = await _inventarioPrivadoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(InventarioPrivadoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(InventarioPrivadoController_DeleteConfirmed_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.InventarioPrivadoViewModel());
        await _inventarioPrivadoService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(InventarioPrivadoController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsInventarioPrivadoViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.InventarioPrivadoViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(InventarioPrivadoController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsInventarioPrivadoViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.InventarioPrivadoViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.UsuarioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.UsuarioOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
