// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(ResenaVendedorController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(ResenaVendedorController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class ResenaVendedorController : Controller
{
    /*PROTECTED REGION ID(ResenaVendedorController_dependencies) START*/
    private readonly ResenaVendedorService _resenaVendedorService;
    private readonly ILogger<ResenaVendedorController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public ResenaVendedorController(ResenaVendedorService resenaVendedorService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<ResenaVendedorController>? logger = null)
    {
        _resenaVendedorService = resenaVendedorService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<ResenaVendedorController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(ResenaVendedorController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(ResenaVendedorController_Index_GET_body) START*/

        var entities = await _resenaVendedorService.ReadAllAsync();
        return View(entities.Select(ResenaVendedorAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ResenaVendedorController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_Details_GET_body) START*/

        var entity = await _resenaVendedorService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ResenaVendedorAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ResenaVendedorController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(ResenaVendedorController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.ResenaVendedorViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ResenaVendedorController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.ResenaVendedorViewModel model)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new ResenaVendedorService(unitOfWork);



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.CompradorId is int selectedComprador &&
            !model.CompradorOptions.Any(option => option.Value == Convert.ToString(selectedComprador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CompradorId", "Selecciona un registro existente de Usuario.");
        if (model.CompradorId is null) ModelState.AddModelError("CompradorId", "Debes seleccionar una opción.");
        if (model.VendedorId is int selectedVendedor &&
            !model.VendedorOptions.Any(option => option.Value == Convert.ToString(selectedVendedor, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VendedorId", "Selecciona un registro existente de Usuario.");
        if (model.VendedorId is null) ModelState.AddModelError("VendedorId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.ResenaVendedor.Create");
        try
        {

            var entity = await service.NewAsync(model.Puntuacion, model.Comentario, model.Fecha, model.Oculta, model.CompradorId.GetValueOrDefault(), model.VendedorId.GetValueOrDefault());
            entity.Comprador = model.CompradorId is int referenceComprador
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceComprador) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.Vendedor = model.VendedorId is int referenceVendedor
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceVendedor) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;

            await service.ModifierResenaVendedorDefaultAsync(entity);

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
    /*PROTECTED REGION ID(ResenaVendedorController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_Edit_GET_body) START*/

        var entity = await _resenaVendedorService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = ResenaVendedorAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ResenaVendedorController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.ResenaVendedorViewModel model)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new ResenaVendedorService(unitOfWork);
        var entity = await service.ReadOIDResenaVendedorDefaultAsync(id);
        if (entity is null) return NotFound();


        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.CompradorId is int selectedComprador &&
            !model.CompradorOptions.Any(option => option.Value == Convert.ToString(selectedComprador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CompradorId", "Selecciona un registro existente de Usuario.");
        if (model.CompradorId is null) ModelState.AddModelError("CompradorId", "Debes seleccionar una opción.");
        if (model.VendedorId is int selectedVendedor &&
            !model.VendedorOptions.Any(option => option.Value == Convert.ToString(selectedVendedor, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VendedorId", "Selecciona un registro existente de Usuario.");
        if (model.VendedorId is null) ModelState.AddModelError("VendedorId", "Debes seleccionar una opción.");


        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.ResenaVendedor.Edit");
        try
        {

            await service.ModifyAsync(id, model.Puntuacion, model.Comentario, model.Fecha, model.Oculta);
            entity.Comprador = model.CompradorId is int referenceComprador
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceComprador) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.Vendedor = model.VendedorId is int referenceVendedor
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceVendedor) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;

            await service.ModifierResenaVendedorDefaultAsync(entity);

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
    /*PROTECTED REGION ID(ResenaVendedorController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_Delete_GET_body) START*/

        var entity = await _resenaVendedorService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ResenaVendedorAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ResenaVendedorController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.ResenaVendedorViewModel());
        await _resenaVendedorService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ResenaVendedorController_FiltrarResenasVisiblesPorVendedor_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarResenasVisiblesPorVendedor(int p_vendedorId)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_FiltrarResenasVisiblesPorVendedor_GET_body) START*/

        var result = await _resenaVendedorService.FiltrarResenasVisiblesPorVendedorAsync(p_vendedorId);
        return Ok(result.Select(ResenaVendedorAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(ResenaVendedorController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsResenaVendedorViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.ResenaVendedorViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ResenaVendedorController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsResenaVendedorViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.ResenaVendedorViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.UsuarioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.CompradorOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.VendedorOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
