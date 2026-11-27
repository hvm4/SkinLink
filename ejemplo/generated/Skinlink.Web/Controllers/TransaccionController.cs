// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(TransaccionController_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(TransaccionController_class_attributes) ENABLED START*/
[Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Todos)]
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
    /*PROTECTED REGION ID(TransaccionController_Index_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
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
    /*PROTECTED REGION ID(TransaccionController_Details_GET_body) ENABLED START*/

        var entity = await _transaccionService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = TransaccionAssembler.ToViewModel(entity);
        // Un usuario solo puede ver los movimientos de su propio monedero; el administrador, cualquiera.
        if (!Skinlink.Web.Utils.Security.UsuarioClaimsExtensions.EsAdministrador(User)
            && (model.MonederoId is not int monederoId
                || !await Skinlink.Web.Utils.CompraventaWebHelper.PuedeUsarMonederoAsync(_formUnitOfWorkFactory, User, monederoId)))
            return Forbid();
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(TransaccionController_Create_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
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
    /*PROTECTED REGION ID(TransaccionController_Create_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.TransaccionViewModel model)
    {
    /*PROTECTED REGION ID(TransaccionController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::ejemplo.Application.Services.TransaccionService(unitOfWork);





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


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Transaccion.Create");


            var entity = await service.NewAsync(model.Tipo, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.MontoValor, Divisa = model.MontoDivisa }, model.Fecha, model.Concepto, model.MonederoId.GetValueOrDefault());

            await service.ApplyTransaccionRelationsAsync(entity, p_monedero_id: model.MonederoId, p_ventaCobro_id: model.VentaCobroId, p_ventaPago_id: model.VentaPagoId);

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
    /*PROTECTED REGION ID(TransaccionController_Edit_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
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
    /*PROTECTED REGION ID(TransaccionController_Edit_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.TransaccionViewModel model)
    {
    /*PROTECTED REGION ID(TransaccionController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::ejemplo.Application.Services.TransaccionService(unitOfWork);

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


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.Transaccion.Edit");


            await service.ModifyAsync(id, model.Tipo, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.MontoValor, Divisa = model.MontoDivisa }, model.Fecha, model.Concepto);
            entity = await service.ReadOIDTransaccionDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyTransaccionRelationsAsync(entity, p_monedero_id: model.MonederoId, p_ventaCobro_id: model.VentaCobroId, p_ventaPago_id: model.VentaPagoId);

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
    /*PROTECTED REGION ID(TransaccionController_Delete_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
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
    /*PROTECTED REGION ID(TransaccionController_DeleteConfirmed_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(TransaccionController_DeleteConfirmed_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.TransaccionViewModel());
        try
        {
            await _transaccionService.DestroyAsync(id);
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

    [HttpGet]
    /*PROTECTED REGION ID(TransaccionController_FiltrarTransaccionesPorTipo_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarTransaccionesPorTipo(int p_usuarioId, TipoTransaccion p_tipo)
    {
    /*PROTECTED REGION ID(TransaccionController_FiltrarTransaccionesPorTipo_GET_body) ENABLED START*/

        var model = new TransaccionFiltroTipoViewModel { Tipo = p_tipo };

        // Sin parámetros en la URL: solo se muestra el formulario.
        if (!Request.Query.ContainsKey("p_tipo"))
            return View(model);

        // Administrador: 0 = movimientos de todos. Usuario normal: solo su propio monedero.
        int usuarioFiltro = 0;
        if (!User.EsAdministrador())
        {
            var usuarioId = User.GetUsuarioId();
            if (usuarioId is null) return Challenge();
            usuarioFiltro = usuarioId.Value;
            model.SoloMisMovimientos = true;
        }

        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        // Se cargan los monederos en el mismo contexto para que EF enlace Transaccion.Monedero.
        await unitOfWork.MonederoRepository.ReadAllAsync();
        var service = new TransaccionService(unitOfWork);
        var result = await service.FiltrarTransaccionesPorTipoAsync(usuarioFiltro, p_tipo);

        model.Buscado = true;
        model.Resultados = result
            .OrderByDescending(x => x.Fecha)
            .Select(TransaccionAssembler.ToViewModel)
            .ToList();
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(TransaccionController_FiltrarTransaccionesPorRangoFechas_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarTransaccionesPorRangoFechas(int p_usuarioId, DateTime p_fechaInicio, DateTime p_fechaFin)
    {
    /*PROTECTED REGION ID(TransaccionController_FiltrarTransaccionesPorRangoFechas_GET_body) ENABLED START*/

        var model = new TransaccionFiltroFechasViewModel
        {
            FechaInicio = DateTime.Today.AddDays(-30),
            FechaFin = DateTime.Today
        };

        // Sin parámetros en la URL: solo se muestra el formulario.
        if (!Request.Query.ContainsKey("p_fechaInicio") && !Request.Query.ContainsKey("p_fechaFin"))
            return View(model);

        if (p_fechaInicio == default || p_fechaFin == default)
        {
            ModelState.AddModelError(string.Empty, "Indica la fecha de inicio y la de fin.");
            return View(model);
        }

        model.FechaInicio = p_fechaInicio.Date;
        model.FechaFin = p_fechaFin.Date;
        if (model.FechaInicio > model.FechaFin)
        {
            ModelState.AddModelError(string.Empty, "La fecha de inicio no puede ser posterior a la fecha de fin.");
            return View(model);
        }

        // Administrador: 0 = movimientos de todos. Usuario normal: solo su propio monedero.
        int usuarioFiltro = 0;
        if (!User.EsAdministrador())
        {
            var usuarioId = User.GetUsuarioId();
            if (usuarioId is null) return Challenge();
            usuarioFiltro = usuarioId.Value;
            model.SoloMisMovimientos = true;
        }

        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        // Se cargan los monederos en el mismo contexto para que EF enlace Transaccion.Monedero.
        await unitOfWork.MonederoRepository.ReadAllAsync();
        var service = new TransaccionService(unitOfWork);

        // El filtro usa "<=": se pasa el último instante del día de fin para incluir ese día entero.
        IEnumerable<Transaccion> visibles = await service.FiltrarTransaccionesPorRangoFechasAsync(
            usuarioFiltro, model.FechaInicio, model.FechaFin.AddDays(1).AddTicks(-1));

        model.Buscado = true;
        model.Resultados = visibles
            .OrderByDescending(t => t.Fecha)
            .Select(TransaccionAssembler.ToViewModel)
            .ToList();
        return View(model);
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
