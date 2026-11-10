// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(AdministradorController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;
using Skinlink.Web.Utils.Security;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(AdministradorController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class AdministradorController : Controller
{
    /*PROTECTED REGION ID(AdministradorController_dependencies) START*/
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly AdministradorService _administradorService;
    private readonly UsuarioService _usuarioService;
    private readonly ILogger<AdministradorController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public AdministradorController(AdministradorService administradorService, UsuarioService usuarioService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ejemplo.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<AdministradorController>? logger = null)
    {
        _administradorService = administradorService;
        _usuarioService = usuarioService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<AdministradorController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(AdministradorController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(AdministradorController_Index_GET_body) START*/

        var entities = await _administradorService.ReadAllAsync();
        return View(entities.Select(AdministradorAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(AdministradorController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(AdministradorController_Details_GET_body) START*/

        var entity = await _administradorService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(AdministradorAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(AdministradorController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(AdministradorController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.AdministradorViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AdministradorController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.AdministradorViewModel model)
    {
    /*PROTECTED REGION ID(AdministradorController_Create_POST_body) ENABLED START*/

        ModelState.Remove(nameof(model.FechaUltimaRecompensa)); // Campo fuera del contrato Create de Application.
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new AdministradorService(unitOfWork);



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.MonederoId is int selectedMonedero &&
            !model.MonederoOptions.Any(option => option.Value == Convert.ToString(selectedMonedero, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("MonederoId", "Selecciona un registro existente de Monedero.");
        if (model.MonederoId is null) ModelState.AddModelError("MonederoId", "Debes seleccionar una opción.");
        if (model.InventarioId is int selectedInventario &&
            !model.InventarioOptions.Any(option => option.Value == Convert.ToString(selectedInventario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("InventarioId", "Selecciona un registro existente de InventarioPrivado.");
        if (model.InventarioId is null) ModelState.AddModelError("InventarioId", "Debes seleccionar una opción.");

        if (string.IsNullOrEmpty(model.Contrasena)) ModelState.AddModelError(nameof(model.Contrasena), "Debes indicar una contraseña.");
        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Administrador.Create");
        try
        {

            var monederoSel = await unitOfWork.MonederoRepository.ReadAsync(model.MonederoId.GetValueOrDefault()) ?? throw new InvalidOperationException("El registro seleccionado de Monedero ya no existe.");
            var inventarioSel = await unitOfWork.InventarioPrivadoRepository.ReadAsync(model.InventarioId.GetValueOrDefault()) ?? throw new InvalidOperationException("El registro seleccionado de InventarioPrivado ya no existe.");
            var entity = await service.NewAsync(model.NombreUsuario, model.Correo, model.Contrasena!, model.FechaNacimiento, model.Estado, model.Nivel, model.Experiencia, model.FechaUltimoLogin, model.TemaModoOscuro, model.Idioma, model.Verificado, monederoSel, inventarioSel, model.CodigoAuditor);
            entity.Monedero = model.MonederoId is int referenceMonedero
                ? await unitOfWork.MonederoRepository.ReadAsync(referenceMonedero) ?? throw new InvalidOperationException("El registro seleccionado de Monedero ya no existe.") : null!;
            entity.Inventario = model.InventarioId is int referenceInventario
                ? await unitOfWork.InventarioPrivadoRepository.ReadAsync(referenceInventario) ?? throw new InvalidOperationException("El registro seleccionado de InventarioPrivado ya no existe.") : null!;

            await service.ModifierAdministradorDefaultAsync(entity);

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
    /*PROTECTED REGION ID(AdministradorController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(AdministradorController_Edit_GET_body) START*/

        var entity = await _administradorService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = AdministradorAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AdministradorController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.AdministradorViewModel model)
    {
    /*PROTECTED REGION ID(AdministradorController_Edit_POST_body) ENABLED START*/

        ModelState.Remove(nameof(model.FechaUltimaRecompensa)); // Campo fuera del contrato Edit de Application.
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new AdministradorService(unitOfWork);
        var entity = await service.ReadOIDAdministradorDefaultAsync(id);
        if (entity is null) return NotFound();


        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.MonederoId is int selectedMonedero &&
            !model.MonederoOptions.Any(option => option.Value == Convert.ToString(selectedMonedero, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("MonederoId", "Selecciona un registro existente de Monedero.");
        if (model.MonederoId is null) ModelState.AddModelError("MonederoId", "Debes seleccionar una opción.");
        if (model.InventarioId is int selectedInventario &&
            !model.InventarioOptions.Any(option => option.Value == Convert.ToString(selectedInventario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("InventarioId", "Selecciona un registro existente de InventarioPrivado.");
        if (model.InventarioId is null) ModelState.AddModelError("InventarioId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Administrador.Edit");
        try
        {

            await service.ModifyAsync(id, model.NombreUsuario, model.Correo, (string.IsNullOrEmpty(model.Contrasena) ? entity.Contrasena : model.Contrasena), model.FechaNacimiento, model.Estado, model.Nivel, model.Experiencia, model.FechaUltimoLogin, model.TemaModoOscuro, model.Idioma, model.Verificado, null!, null!, model.CodigoAuditor);
            entity.Monedero = model.MonederoId is int referenceMonedero
                ? await unitOfWork.MonederoRepository.ReadAsync(referenceMonedero) ?? throw new InvalidOperationException("El registro seleccionado de Monedero ya no existe.") : null!;
            entity.Inventario = model.InventarioId is int referenceInventario
                ? await unitOfWork.InventarioPrivadoRepository.ReadAsync(referenceInventario) ?? throw new InvalidOperationException("El registro seleccionado de InventarioPrivado ya no existe.") : null!;

            await service.ModifierAdministradorDefaultAsync(entity);

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
    /*PROTECTED REGION ID(AdministradorController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(AdministradorController_Delete_GET_body) START*/

        var entity = await _administradorService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(AdministradorAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(AdministradorController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(AdministradorController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.AdministradorViewModel());
        await _administradorService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(AdministradorController_FiltrarUsuariosPorEstado_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarUsuariosPorEstado(EstadoCuenta p_estado)
    {
    /*PROTECTED REGION ID(AdministradorController_FiltrarUsuariosPorEstado_GET_body) START*/

        var result = await _usuarioService.FiltrarUsuariosPorEstadoAsync(p_estado);
        return Ok(result.Select(UsuarioAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(AdministradorController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsAdministradorViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.AdministradorViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(AdministradorController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsAdministradorViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.AdministradorViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.MonederoRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.InventarioPrivadoRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.MonederoOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.InventarioOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}