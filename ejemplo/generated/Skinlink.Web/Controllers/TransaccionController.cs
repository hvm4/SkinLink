// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(TransaccionController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(TransaccionController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class TransaccionController : Controller
{
    /*PROTECTED REGION ID(TransaccionController_dependencies) START*/
    private readonly TransaccionService _transaccionService;
    private readonly ILogger<TransaccionController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public TransaccionController(TransaccionService transaccionService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ILogger<TransaccionController>? logger = null)
    {
        _transaccionService = transaccionService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;

        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<TransaccionController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(TransaccionController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(TransaccionController_Index_GET_body) START*/

        var entities = await _transaccionService.ReadAllAsync();
        return View(entities.Select(TransaccionAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(TransaccionController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(TransaccionController_Details_GET_body) START*/

        var entity = await _transaccionService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(TransaccionAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(TransaccionController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(TransaccionController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.TransaccionViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(TransaccionController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.TransaccionViewModel model)
    {
    /*PROTECTED REGION ID(TransaccionController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new TransaccionService(unitOfWork);



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.MonederoId is int selectedMonedero &&
            !model.MonederoOptions.Any(option => option.Value == Convert.ToString(selectedMonedero, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("MonederoId", "Selecciona un registro existente de Monedero.");
        if (model.MonederoId is null) ModelState.AddModelError("MonederoId", "Debes seleccionar una opción.");
        if (model.VentaCobroId is int selectedVentaCobro &&
            !model.VentaCobroOptions.Any(option => option.Value == Convert.ToString(selectedVentaCobro, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VentaCobroId", "Selecciona un registro existente de Venta.");

        if (model.VentaPagoId is int selectedVentaPago &&
            !model.VentaPagoOptions.Any(option => option.Value == Convert.ToString(selectedVentaPago, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VentaPagoId", "Selecciona un registro existente de Venta.");



        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Transaccion.Create");
        try
        {

            var entity = await service.NewAsync(model.Tipo, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.MontoValor, Divisa = model.MontoDivisa }, model.Fecha, model.Concepto, model.MonederoId.GetValueOrDefault());
            entity.Monedero = model.MonederoId is int referenceMonedero
                ? await unitOfWork.MonederoRepository.ReadAsync(referenceMonedero) ?? throw new InvalidOperationException("El registro seleccionado de Monedero ya no existe.") : null!;
            entity.VentaCobro = model.VentaCobroId is int referenceVentaCobro
                ? await unitOfWork.VentaRepository.ReadAsync(referenceVentaCobro) ?? throw new InvalidOperationException("El registro seleccionado de Venta ya no existe.") : null!;
            entity.VentaPago = model.VentaPagoId is int referenceVentaPago
                ? await unitOfWork.VentaRepository.ReadAsync(referenceVentaPago) ?? throw new InvalidOperationException("El registro seleccionado de Venta ya no existe.") : null!;

            await service.ModifierTransaccionDefaultAsync(entity);

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
    /*PROTECTED REGION ID(TransaccionController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(TransaccionController_Edit_GET_body) START*/

        var entity = await _transaccionService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = TransaccionAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(TransaccionController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.TransaccionViewModel model)
    {
    /*PROTECTED REGION ID(TransaccionController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new TransaccionService(unitOfWork);
        var entity = await service.ReadOIDTransaccionDefaultAsync(id);
        if (entity is null) return NotFound();


        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.MonederoId is int selectedMonedero &&
            !model.MonederoOptions.Any(option => option.Value == Convert.ToString(selectedMonedero, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("MonederoId", "Selecciona un registro existente de Monedero.");
        if (model.MonederoId is null) ModelState.AddModelError("MonederoId", "Debes seleccionar una opción.");
        if (model.VentaCobroId is int selectedVentaCobro &&
            !model.VentaCobroOptions.Any(option => option.Value == Convert.ToString(selectedVentaCobro, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VentaCobroId", "Selecciona un registro existente de Venta.");

        if (model.VentaPagoId is int selectedVentaPago &&
            !model.VentaPagoOptions.Any(option => option.Value == Convert.ToString(selectedVentaPago, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VentaPagoId", "Selecciona un registro existente de Venta.");



        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Transaccion.Edit");
        try
        {

            await service.ModifyAsync(id, model.Tipo, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.MontoValor, Divisa = model.MontoDivisa }, model.Fecha, model.Concepto);
            entity.Monedero = model.MonederoId is int referenceMonedero
                ? await unitOfWork.MonederoRepository.ReadAsync(referenceMonedero) ?? throw new InvalidOperationException("El registro seleccionado de Monedero ya no existe.") : null!;
            entity.VentaCobro = model.VentaCobroId is int referenceVentaCobro
                ? await unitOfWork.VentaRepository.ReadAsync(referenceVentaCobro) ?? throw new InvalidOperationException("El registro seleccionado de Venta ya no existe.") : null!;
            entity.VentaPago = model.VentaPagoId is int referenceVentaPago
                ? await unitOfWork.VentaRepository.ReadAsync(referenceVentaPago) ?? throw new InvalidOperationException("El registro seleccionado de Venta ya no existe.") : null!;

            await service.ModifierTransaccionDefaultAsync(entity);

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
    /*PROTECTED REGION ID(TransaccionController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(TransaccionController_Delete_GET_body) START*/

        var entity = await _transaccionService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(TransaccionAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(TransaccionController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(TransaccionController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.TransaccionViewModel());
        await _transaccionService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(TransaccionController_FiltrarTransaccionesPorTipo_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarTransaccionesPorTipo(TipoTransaccion p_tipo)
    {
    /*PROTECTED REGION ID(TransaccionController_FiltrarTransaccionesPorTipo_GET_body) START*/

        var result = await _transaccionService.FiltrarTransaccionesPorTipoAsync(p_tipo);
        return Ok(result.Select(TransaccionAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(TransaccionController_FiltrarTransaccionesPorRangoFechas_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarTransaccionesPorRangoFechas(DateTime p_fechaInicio, DateTime p_fechaFin)
    {
    /*PROTECTED REGION ID(TransaccionController_FiltrarTransaccionesPorRangoFechas_GET_body) START*/

        var result = await _transaccionService.FiltrarTransaccionesPorRangoFechasAsync(p_fechaInicio, p_fechaFin);
        return Ok(result.Select(TransaccionAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(TransaccionController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsTransaccionViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.TransaccionViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(TransaccionController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsTransaccionViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.TransaccionViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.MonederoRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.VentaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.MonederoOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.VentaCobroOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.VentaPagoOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
