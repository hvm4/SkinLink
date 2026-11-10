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
[Authorize]
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
    /*PROTECTED REGION ID(InventarioPrivadoController_Create_GET_attributes) START*/
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
    /*PROTECTED REGION ID(InventarioPrivadoController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.InventarioPrivadoViewModel model)
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new InventarioPrivadoService(unitOfWork);

        model.Activos ??= new();
        if (model.Activos.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");
        if (model.Activos is null) { model.Activos = new(); ModelState.AddModelError(nameof(model.Activos), "Debes indicar las líneas."); }
        if (model.Activos.Count < 0 || model.Activos.Count > 500)
            ModelState.AddModelError(nameof(model.Activos), "Número de líneas no válido (máximo 500).");

        for (var index = 0; index < model.Activos.Count; index++)
        {
            var row = model.Activos[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!))
                ModelState.AddModelError($"Activos[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.VentaId is int selectedVenta &&
            !row.VentaOptions.Any(option => option.Value == Convert.ToString(selectedVenta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Activos[{index}].VentaId", "Selecciona un registro existente de Venta.");

        if (row.IntercambioOfrecidoId is int selectedIntercambioOfrecido &&
            !row.IntercambioOfrecidoOptions.Any(option => option.Value == Convert.ToString(selectedIntercambioOfrecido, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Activos[{index}].IntercambioOfrecidoId", "Selecciona un registro existente de Intercambio.");

        if (row.IntercambioSolicitadoId is int selectedIntercambioSolicitado &&
            !row.IntercambioSolicitadoOptions.Any(option => option.Value == Convert.ToString(selectedIntercambioSolicitado, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Activos[{index}].IntercambioSolicitadoId", "Selecciona un registro existente de Intercambio.");

        }

        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.InventarioPrivado.Create");
        try
        {
            var submittedActivos = new List<ejemplo.Domain.Entities.ActivoDigital>();
            foreach (var row in model.Activos)
            {
                var child = ActivoDigitalAssembler.Create(row);
                ActivoDigitalAssembler.ApplyToEntity(row, child);
            child.Venta = row.VentaId is int referenceVenta
                ? await unitOfWork.VentaRepository.ReadAsync(referenceVenta) ?? throw new InvalidOperationException("El registro seleccionado de Venta ya no existe.") : null!;
            child.IntercambioOfrecido = row.IntercambioOfrecidoId is int referenceIntercambioOfrecido
                ? await unitOfWork.IntercambioRepository.ReadAsync(referenceIntercambioOfrecido) ?? throw new InvalidOperationException("El registro seleccionado de Intercambio ya no existe.") : null!;
            child.IntercambioSolicitado = row.IntercambioSolicitadoId is int referenceIntercambioSolicitado
                ? await unitOfWork.IntercambioRepository.ReadAsync(referenceIntercambioSolicitado) ?? throw new InvalidOperationException("El registro seleccionado de Intercambio ya no existe.") : null!;
                submittedActivos.Add(child);
            }
            var entity = await service.NewAsync(new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.ValorEstimadoTotalValor, Divisa = model.ValorEstimadoTotalDivisa }, model.EsPublico, model.UsuarioId.GetValueOrDefault(), submittedActivos);
            entity.Usuario = model.UsuarioId is int referenceUsuario
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceUsuario) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            
            for (var index = 0; index < submittedActivos.Count; index++)
            {
                var child = submittedActivos[index];
                child.Inventario = entity;
                if (!entity.Activos.Contains(child)) entity.Activos.Add(child);
            }
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
    /*PROTECTED REGION ID(InventarioPrivadoController_Edit_GET_attributes) START*/
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
    /*PROTECTED REGION ID(InventarioPrivadoController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.InventarioPrivadoViewModel model)
    {
    /*PROTECTED REGION ID(InventarioPrivadoController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new InventarioPrivadoService(unitOfWork);
        var entity = await service.ReadOIDInventarioPrivadoDefaultAsync(id);
        if (entity is null) return NotFound();
        model.Activos ??= new();
        if (model.Activos.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.UsuarioId is int selectedUsuario &&
            !model.UsuarioOptions.Any(option => option.Value == Convert.ToString(selectedUsuario, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("UsuarioId", "Selecciona un registro existente de Usuario.");
        if (model.UsuarioId is null) ModelState.AddModelError("UsuarioId", "Debes seleccionar una opción.");
        if (model.Activos is null) { model.Activos = new(); ModelState.AddModelError(nameof(model.Activos), "Debes indicar las líneas."); }
        if (model.Activos.Count < 0 || model.Activos.Count > 500)
            ModelState.AddModelError(nameof(model.Activos), "Número de líneas no válido (máximo 500).");
        var seenActivos = new HashSet<int>();
        for (var index = 0; index < model.Activos.Count; index++)
        {
            var row = model.Activos[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!) &&
                (!entity.Activos.Any(child => child.Id.Equals(row.Id)) || !seenActivos.Add(row.Id)))
                ModelState.AddModelError($"Activos[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.VentaId is int selectedVenta &&
            !row.VentaOptions.Any(option => option.Value == Convert.ToString(selectedVenta, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Activos[{index}].VentaId", "Selecciona un registro existente de Venta.");

        if (row.IntercambioOfrecidoId is int selectedIntercambioOfrecido &&
            !row.IntercambioOfrecidoOptions.Any(option => option.Value == Convert.ToString(selectedIntercambioOfrecido, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Activos[{index}].IntercambioOfrecidoId", "Selecciona un registro existente de Intercambio.");

        if (row.IntercambioSolicitadoId is int selectedIntercambioSolicitado &&
            !row.IntercambioSolicitadoOptions.Any(option => option.Value == Convert.ToString(selectedIntercambioSolicitado, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Activos[{index}].IntercambioSolicitadoId", "Selecciona un registro existente de Intercambio.");

        }

        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.InventarioPrivado.Edit");
        try
        {
            var submittedActivos = new List<ejemplo.Domain.Entities.ActivoDigital>();
            foreach (var row in model.Activos)
            {
                var child = entity.Activos.SingleOrDefault(item => item.Id.Equals(row.Id)) ?? ActivoDigitalAssembler.Create(row);
                ActivoDigitalAssembler.ApplyToEntity(row, child);
            child.Venta = row.VentaId is int referenceVenta
                ? await unitOfWork.VentaRepository.ReadAsync(referenceVenta) ?? throw new InvalidOperationException("El registro seleccionado de Venta ya no existe.") : null!;
            child.IntercambioOfrecido = row.IntercambioOfrecidoId is int referenceIntercambioOfrecido
                ? await unitOfWork.IntercambioRepository.ReadAsync(referenceIntercambioOfrecido) ?? throw new InvalidOperationException("El registro seleccionado de Intercambio ya no existe.") : null!;
            child.IntercambioSolicitado = row.IntercambioSolicitadoId is int referenceIntercambioSolicitado
                ? await unitOfWork.IntercambioRepository.ReadAsync(referenceIntercambioSolicitado) ?? throw new InvalidOperationException("El registro seleccionado de Intercambio ya no existe.") : null!;
                submittedActivos.Add(child);
            }
            await service.ModifyAsync(id, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.ValorEstimadoTotalValor, Divisa = model.ValorEstimadoTotalDivisa }, model.EsPublico);
            entity.Usuario = model.UsuarioId is int referenceUsuario
                ? await unitOfWork.UsuarioRepository.ReadAsync(referenceUsuario) ?? throw new InvalidOperationException("El registro seleccionado de Usuario ya no existe.") : null!;
            foreach (var removed in entity.Activos.Where(child => !submittedActivos.Contains(child)).ToList()) entity.Activos.Remove(removed);
            for (var index = 0; index < submittedActivos.Count; index++)
            {
                var child = submittedActivos[index];
                child.Inventario = entity;
                if (!entity.Activos.Contains(child)) entity.Activos.Add(child);
            }
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
    /*PROTECTED REGION ID(InventarioPrivadoController_Delete_GET_attributes) START*/
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
    /*PROTECTED REGION ID(InventarioPrivadoController_DeleteConfirmed_POST_attributes) START*/
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
        if (model.Activos.Count > 500) { ModelState.AddModelError(nameof(model.Activos), "Máximo 500 líneas."); return; }
        var options0 = (await unitOfWork.UsuarioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.NombreUsuario, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options1 = (await unitOfWork.VentaRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        var options2 = (await unitOfWork.IntercambioRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();
        model.UsuarioOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        foreach (var row in model.Activos)
        {
        row.VentaOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        row.IntercambioOfrecidoOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        row.IntercambioSolicitadoOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        }
        model.ActivosTemplate.VentaOptions = options1.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.ActivosTemplate.IntercambioOfrecidoOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        model.ActivosTemplate.IntercambioSolicitadoOptions = options2.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
    }
    /*PROTECTED REGION END*/

}
