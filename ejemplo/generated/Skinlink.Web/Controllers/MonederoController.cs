// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(MonederoController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(MonederoController_class_attributes) ENABLED START*/
// Seguridad (Miembro 1): cualquier cuenta activa; las cuentas suspendidas no tienen rol.
// Las acciones con [AllowAnonymous] siguen siendo públicas.
[Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Todos)]
/*PROTECTED REGION END*/
public partial class MonederoController : Controller
{
    /*PROTECTED REGION ID(MonederoController_dependencies) START*/
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly MonederoService _monederoService;
    private readonly ILogger<MonederoController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public MonederoController(MonederoService monederoService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ejemplo.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<MonederoController>? logger = null)
    {
        _monederoService = monederoService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<MonederoController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(MonederoController_Index_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)] // los listados completos son solo del administrador
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(MonederoController_Index_GET_body) START*/

        var entities = await _monederoService.ReadAllAsync();
        return View(entities.Select(MonederoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MonederoController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(MonederoController_Details_GET_body) ENABLED START*/

        // Un usuario solo puede ver su propio monedero; el administrador, cualquiera.
        if (!await Skinlink.Web.Utils.CompraventaWebHelper.PuedeUsarMonederoAsync(_formUnitOfWorkFactory, User, id)) return Forbid();
        var entity = await _monederoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(MonederoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(MonederoController_Create_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(MonederoController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.MonederoViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_Create_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.MonederoViewModel model)
    {
    /*PROTECTED REGION ID(MonederoController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::ejemplo.Application.Services.MonederoService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Monedero.Create");


            var entity = await service.NewAsync(new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.SaldoDisponibleValor, Divisa = model.SaldoDisponibleDivisa }, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.SaldoPendienteValor, Divisa = model.SaldoPendienteDivisa }, model.UsuarioId.GetValueOrDefault());

            await service.ApplyMonederoRelationsAsync(entity, p_usuario_id: model.UsuarioId);

            await service.ModifierMonederoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(MonederoController_Edit_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(MonederoController_Edit_GET_body) START*/

        var entity = await _monederoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = MonederoAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_Edit_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.MonederoViewModel model)
    {
    /*PROTECTED REGION ID(MonederoController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::ejemplo.Application.Services.MonederoService(unitOfWork);

        var entity = await service.ReadOIDMonederoDefaultAsync(id);
        if (entity is null) return NotFound();



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");




        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Monedero.Edit");


            await service.ModifyAsync(id, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.SaldoDisponibleValor, Divisa = model.SaldoDisponibleDivisa }, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.SaldoPendienteValor, Divisa = model.SaldoPendienteDivisa });
            entity = await service.ReadOIDMonederoDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyMonederoRelationsAsync(entity, p_usuario_id: model.UsuarioId);

            await service.ModifierMonederoDefaultAsync(entity);


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
    /*PROTECTED REGION ID(MonederoController_Delete_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(MonederoController_Delete_GET_body) START*/

        var entity = await _monederoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(MonederoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(MonederoController_DeleteConfirmed_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(MonederoController_DeleteConfirmed_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.MonederoViewModel());
        try
        {
            await _monederoService.DestroyAsync(id);
            TempData["ExitoBorrado"] = "Registro eliminado.";
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            TempData["ErrorBorrado"] = this.MensajeDeErrorAlEliminar(error);
            return RedirectToAction(nameof(Delete), new { id });
        }
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(MonederoController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsMonederoViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.MonederoViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(MonederoController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsMonederoViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.MonederoViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
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
