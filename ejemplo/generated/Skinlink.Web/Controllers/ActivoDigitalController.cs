// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(ActivoDigitalController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(ActivoDigitalController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class ActivoDigitalController : Controller
{
    /*PROTECTED REGION ID(ActivoDigitalController_dependencies) START*/
    private readonly ActivoDigitalService _activoDigitalService;
    private readonly ILogger<ActivoDigitalController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public ActivoDigitalController(ActivoDigitalService activoDigitalService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<ActivoDigitalController>? logger = null)
    {
        _activoDigitalService = activoDigitalService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<ActivoDigitalController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(ActivoDigitalController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(ActivoDigitalController_Index_GET_body) START*/

        var entities = await _activoDigitalService.ReadAllAsync();
        return View(entities.Select(ActivoDigitalAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ActivoDigitalController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_Details_GET_body) START*/

        var entity = await _activoDigitalService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ActivoDigitalAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ActivoDigitalController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(ActivoDigitalController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.ActivoDigitalViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ActivoDigitalController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.ActivoDigitalViewModel model)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new ActivoDigitalService(unitOfWork);



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.InventarioId is int selectedInventario &&
            !model.InventarioOptions.Any(option => option.Value == Convert.ToString(selectedInventario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("InventarioId", "Selecciona un registro existente de InventarioPrivado.");
        if (model.InventarioId is null) ModelState.AddModelError("InventarioId", "Debes seleccionar una opción.");
        if (model.VentaId is int selectedVenta &&
            !model.VentaOptions.Any(option => option.Value == Convert.ToString(selectedVenta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VentaId", "Selecciona un registro existente de Venta.");

        if (model.IntercambioOfrecidoId is int selectedIntercambioOfrecido &&
            !model.IntercambioOfrecidoOptions.Any(option => option.Value == Convert.ToString(selectedIntercambioOfrecido, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("IntercambioOfrecidoId", "Selecciona un registro existente de Intercambio.");

        if (model.IntercambioSolicitadoId is int selectedIntercambioSolicitado &&
            !model.IntercambioSolicitadoOptions.Any(option => option.Value == Convert.ToString(selectedIntercambioSolicitado, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("IntercambioSolicitadoId", "Selecciona un registro existente de Intercambio.");



        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.ActivoDigital.Create");
        try
        {

            var entity = await service.NewAsync(model.Nombre, model.Coleccion, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioEstimadoValor, Divisa = model.PrecioEstimadoDivisa }, new global::ejemplo.Domain.ValueObjects.AtributosSkin { Rareza = model.AtributosRareza, Condicion = model.AtributosCondicion, FloatVal = model.AtributosFloatVal, Patron = model.AtributosPatron }, model.EnVenta, model.IdentificadorUnico, model.InventarioId.GetValueOrDefault());
            entity.Inventario = model.InventarioId is int referenceInventario
                ? await unitOfWork.InventarioPrivadoRepository.ReadAsync(referenceInventario) ?? throw new InvalidOperationException("El registro seleccionado de InventarioPrivado ya no existe.") : null!;
            entity.Venta = model.VentaId is int referenceVenta
                ? await unitOfWork.VentaRepository.ReadAsync(referenceVenta) ?? throw new InvalidOperationException("El registro seleccionado de Venta ya no existe.") : null!;
            entity.IntercambioOfrecido = model.IntercambioOfrecidoId is int referenceIntercambioOfrecido
                ? await unitOfWork.IntercambioRepository.ReadAsync(referenceIntercambioOfrecido) ?? throw new InvalidOperationException("El registro seleccionado de Intercambio ya no existe.") : null!;
            entity.IntercambioSolicitado = model.IntercambioSolicitadoId is int referenceIntercambioSolicitado
                ? await unitOfWork.IntercambioRepository.ReadAsync(referenceIntercambioSolicitado) ?? throw new InvalidOperationException("El registro seleccionado de Intercambio ya no existe.") : null!;

            await service.ModifierActivoDigitalDefaultAsync(entity);

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
    /*PROTECTED REGION ID(ActivoDigitalController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_Edit_GET_body) START*/

        var entity = await _activoDigitalService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = ActivoDigitalAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ActivoDigitalController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.ActivoDigitalViewModel model)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new ActivoDigitalService(unitOfWork);
        var entity = await service.ReadOIDActivoDigitalDefaultAsync(id);
        if (entity is null) return NotFound();


        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.InventarioId is int selectedInventario &&
            !model.InventarioOptions.Any(option => option.Value == Convert.ToString(selectedInventario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("InventarioId", "Selecciona un registro existente de InventarioPrivado.");
        if (model.InventarioId is null) ModelState.AddModelError("InventarioId", "Debes seleccionar una opción.");
        if (model.VentaId is int selectedVenta &&
            !model.VentaOptions.Any(option => option.Value == Convert.ToString(selectedVenta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VentaId", "Selecciona un registro existente de Venta.");

        if (model.IntercambioOfrecidoId is int selectedIntercambioOfrecido &&
            !model.IntercambioOfrecidoOptions.Any(option => option.Value == Convert.ToString(selectedIntercambioOfrecido, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("IntercambioOfrecidoId", "Selecciona un registro existente de Intercambio.");

        if (model.IntercambioSolicitadoId is int selectedIntercambioSolicitado &&
            !model.IntercambioSolicitadoOptions.Any(option => option.Value == Convert.ToString(selectedIntercambioSolicitado, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("IntercambioSolicitadoId", "Selecciona un registro existente de Intercambio.");



        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.ActivoDigital.Edit");
        try
        {

            await service.ModifyAsync(id, model.Nombre, model.Coleccion, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioEstimadoValor, Divisa = model.PrecioEstimadoDivisa }, new global::ejemplo.Domain.ValueObjects.AtributosSkin { Rareza = model.AtributosRareza, Condicion = model.AtributosCondicion, FloatVal = model.AtributosFloatVal, Patron = model.AtributosPatron }, model.EnVenta, model.IdentificadorUnico);
            entity.Inventario = model.InventarioId is int referenceInventario
                ? await unitOfWork.InventarioPrivadoRepository.ReadAsync(referenceInventario) ?? throw new InvalidOperationException("El registro seleccionado de InventarioPrivado ya no existe.") : null!;
            entity.Venta = model.VentaId is int referenceVenta
                ? await unitOfWork.VentaRepository.ReadAsync(referenceVenta) ?? throw new InvalidOperationException("El registro seleccionado de Venta ya no existe.") : null!;
            entity.IntercambioOfrecido = model.IntercambioOfrecidoId is int referenceIntercambioOfrecido
                ? await unitOfWork.IntercambioRepository.ReadAsync(referenceIntercambioOfrecido) ?? throw new InvalidOperationException("El registro seleccionado de Intercambio ya no existe.") : null!;
            entity.IntercambioSolicitado = model.IntercambioSolicitadoId is int referenceIntercambioSolicitado
                ? await unitOfWork.IntercambioRepository.ReadAsync(referenceIntercambioSolicitado) ?? throw new InvalidOperationException("El registro seleccionado de Intercambio ya no existe.") : null!;

            await service.ModifierActivoDigitalDefaultAsync(entity);

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
    /*PROTECTED REGION ID(ActivoDigitalController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_Delete_GET_body) START*/

        var entity = await _activoDigitalService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ActivoDigitalAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ActivoDigitalController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.ActivoDigitalViewModel());
        await _activoDigitalService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ActivoDigitalController_FiltrarActivosPorRareza_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarActivosPorRareza(Rareza p_rareza)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_FiltrarActivosPorRareza_GET_body) START*/

        var result = await _activoDigitalService.FiltrarActivosPorRarezaAsync(p_rareza);
        return Ok(result.Select(ActivoDigitalAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ActivoDigitalController_FiltrarActivosPorColeccion_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarActivosPorColeccion(string p_coleccion)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_FiltrarActivosPorColeccion_GET_body) START*/

        var result = await _activoDigitalService.FiltrarActivosPorColeccionAsync(p_coleccion);
        return Ok(result.Select(ActivoDigitalAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ActivoDigitalController_FiltrarActivosPorCondicion_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarActivosPorCondicion(Condicion p_condicion)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_FiltrarActivosPorCondicion_GET_body) START*/

        var result = await _activoDigitalService.FiltrarActivosPorCondicionAsync(p_condicion);
        return Ok(result.Select(ActivoDigitalAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ActivoDigitalController_FiltrarActivosEnVenta_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarActivosEnVenta(int p_usuarioId, bool p_enVenta)
    {
    /*PROTECTED REGION ID(ActivoDigitalController_FiltrarActivosEnVenta_GET_body) START*/

        var result = await _activoDigitalService.FiltrarActivosEnVentaAsync(p_usuarioId, p_enVenta);
        return Ok(result.Select(ActivoDigitalAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(ActivoDigitalController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsActivoDigitalViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.ActivoDigitalViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ActivoDigitalController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsActivoDigitalViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.ActivoDigitalViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.InventarioPrivadoRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.VentaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options2 = (await unitOfWork.IntercambioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.InventarioOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.VentaOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.IntercambioOfrecidoOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.IntercambioSolicitadoOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
