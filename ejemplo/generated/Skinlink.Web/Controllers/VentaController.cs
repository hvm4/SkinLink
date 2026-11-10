// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(VentaController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(VentaController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class VentaController : Controller
{
    /*PROTECTED REGION ID(VentaController_dependencies) START*/
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly VentaService _ventaService;
    private readonly ILogger<VentaController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public VentaController(VentaService ventaService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ejemplo.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<VentaController>? logger = null)
    {
        _ventaService = ventaService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<VentaController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(VentaController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(VentaController_Index_GET_body) START*/

        var entities = await _ventaService.ReadAllAsync();
        return View(entities.Select(VentaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(VentaController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(VentaController_Details_GET_body) START*/

        var entity = await _ventaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(VentaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(VentaController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(VentaController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.VentaViewModel();

        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.VentaViewModel model)
    {
    /*PROTECTED REGION ID(VentaController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new VentaService(unitOfWork);



        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.VendedorId is int selectedVendedor &&
            !model.VendedorOptions.Any(option => option.Value == Convert.ToString(selectedVendedor, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VendedorId", "Selecciona un registro existente de Usuario.");
        if (model.VendedorId is null) ModelState.AddModelError("VendedorId", "Debes seleccionar una opción.");
        if (model.CompradorId is int selectedComprador &&
            !model.CompradorOptions.Any(option => option.Value == Convert.ToString(selectedComprador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CompradorId", "Selecciona un registro existente de Usuario.");

        if (model.ActivoDigitalId is int selectedActivoDigital &&
            !model.ActivoDigitalOptions.Any(option => option.Value == Convert.ToString(selectedActivoDigital, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ActivoDigitalId", "Selecciona un registro existente de ActivoDigital.");
        if (model.ActivoDigitalId is null) ModelState.AddModelError("ActivoDigitalId", "Debes seleccionar una opción.");
        if (model.TransaccionCobroId is int selectedTransaccionCobro &&
            !model.TransaccionCobroOptions.Any(option => option.Value == Convert.ToString(selectedTransaccionCobro, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("TransaccionCobroId", "Selecciona un registro existente de Transaccion.");

        if (model.TransaccionPagoId is int selectedTransaccionPago &&
            !model.TransaccionPagoOptions.Any(option => option.Value == Convert.ToString(selectedTransaccionPago, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("TransaccionPagoId", "Selecciona un registro existente de Transaccion.");



        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Venta.Create");
        try
        {

            var entity = await service.NewAsync(new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioVentaValor, Divisa = model.PrecioVentaDivisa }, model.FechaPublicacion, model.EstadoVenta, model.VendedorId.GetValueOrDefault(), model.CompradorId.GetValueOrDefault(), model.ActivoDigitalId.GetValueOrDefault(), model.TransaccionCobroId.GetValueOrDefault(), model.TransaccionPagoId.GetValueOrDefault());
            entity.Vendedor = model.VendedorId is int referenceVendedor
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceVendedor) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.Comprador = model.CompradorId is int referenceComprador
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceComprador) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.ActivoDigital = model.ActivoDigitalId is int referenceActivoDigital
                ? await unitOfWork.ActivoDigitalRepository.ReadAsync(referenceActivoDigital) ?? throw new InvalidOperationException("El registro seleccionado de ActivoDigital ya no existe.") : null!;
            entity.TransaccionCobro = model.TransaccionCobroId is int referenceTransaccionCobro
                ? await unitOfWork.TransaccionRepository.ReadAsync(referenceTransaccionCobro) ?? throw new InvalidOperationException("El registro seleccionado de Transaccion ya no existe.") : null!;
            entity.TransaccionPago = model.TransaccionPagoId is int referenceTransaccionPago
                ? await unitOfWork.TransaccionRepository.ReadAsync(referenceTransaccionPago) ?? throw new InvalidOperationException("El registro seleccionado de Transaccion ya no existe.") : null!;

            await service.ModifierVentaDefaultAsync(entity);

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
    /*PROTECTED REGION ID(VentaController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(VentaController_Edit_GET_body) START*/

        var entity = await _ventaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = VentaAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.VentaViewModel model)
    {
    /*PROTECTED REGION ID(VentaController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new VentaService(unitOfWork);
        var entity = await service.ReadOIDVentaDefaultAsync(id);
        if (entity is null) return NotFound();


        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.VendedorId is int selectedVendedor &&
            !model.VendedorOptions.Any(option => option.Value == Convert.ToString(selectedVendedor, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VendedorId", "Selecciona un registro existente de Usuario.");
        if (model.VendedorId is null) ModelState.AddModelError("VendedorId", "Debes seleccionar una opción.");
        if (model.CompradorId is int selectedComprador &&
            !model.CompradorOptions.Any(option => option.Value == Convert.ToString(selectedComprador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CompradorId", "Selecciona un registro existente de Usuario.");

        if (model.ActivoDigitalId is int selectedActivoDigital &&
            !model.ActivoDigitalOptions.Any(option => option.Value == Convert.ToString(selectedActivoDigital, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("ActivoDigitalId", "Selecciona un registro existente de ActivoDigital.");
        if (model.ActivoDigitalId is null) ModelState.AddModelError("ActivoDigitalId", "Debes seleccionar una opción.");
        if (model.TransaccionCobroId is int selectedTransaccionCobro &&
            !model.TransaccionCobroOptions.Any(option => option.Value == Convert.ToString(selectedTransaccionCobro, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("TransaccionCobroId", "Selecciona un registro existente de Transaccion.");

        if (model.TransaccionPagoId is int selectedTransaccionPago &&
            !model.TransaccionPagoOptions.Any(option => option.Value == Convert.ToString(selectedTransaccionPago, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("TransaccionPagoId", "Selecciona un registro existente de Transaccion.");



        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.Venta.Edit");
        try
        {

            await service.ModifyAsync(id, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioVentaValor, Divisa = model.PrecioVentaDivisa }, model.FechaPublicacion, model.EstadoVenta, model.ActivoDigitalId.GetValueOrDefault());
            entity.Vendedor = model.VendedorId is int referenceVendedor
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceVendedor) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.Comprador = model.CompradorId is int referenceComprador
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceComprador) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            entity.ActivoDigital = model.ActivoDigitalId is int referenceActivoDigital
                ? await unitOfWork.ActivoDigitalRepository.ReadAsync(referenceActivoDigital) ?? throw new InvalidOperationException("El registro seleccionado de ActivoDigital ya no existe.") : null!;
            entity.TransaccionCobro = model.TransaccionCobroId is int referenceTransaccionCobro
                ? await unitOfWork.TransaccionRepository.ReadAsync(referenceTransaccionCobro) ?? throw new InvalidOperationException("El registro seleccionado de Transaccion ya no existe.") : null!;
            entity.TransaccionPago = model.TransaccionPagoId is int referenceTransaccionPago
                ? await unitOfWork.TransaccionRepository.ReadAsync(referenceTransaccionPago) ?? throw new InvalidOperationException("El registro seleccionado de Transaccion ya no existe.") : null!;

            await service.ModifierVentaDefaultAsync(entity);

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
    /*PROTECTED REGION ID(VentaController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(VentaController_Delete_GET_body) START*/

        var entity = await _ventaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(VentaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(VentaController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(VentaController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.VentaViewModel());
        await _ventaService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(VentaController_FiltrarVentasPorEstado_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarVentasPorEstado(EstadoVenta p_estado)
    {
    /*PROTECTED REGION ID(VentaController_FiltrarVentasPorEstado_GET_body) START*/

        var result = await _ventaService.FiltrarVentasPorEstadoAsync(p_estado);
        return Ok(result.Select(VentaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(VentaController_FiltrarVentasPorPrecioMaximo_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarVentasPorPrecioMaximo(double p_precioMaximo)
    {
    /*PROTECTED REGION ID(VentaController_FiltrarVentasPorPrecioMaximo_GET_body) START*/

        var result = await _ventaService.FiltrarVentasPorPrecioMaximoAsync(p_precioMaximo);
        return Ok(result.Select(VentaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(VentaController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsVentaViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.VentaViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(VentaController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsVentaViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.VentaViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {

        var options0 = (await unitOfWork.UsuarioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.ActivoDigitalRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options2 = (await unitOfWork.TransaccionRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Concepto, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.VendedorOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.CompradorOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.ActivoDigitalOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.TransaccionCobroOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.TransaccionPagoOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();

    }
    /*PROTECTED REGION END*/

}
