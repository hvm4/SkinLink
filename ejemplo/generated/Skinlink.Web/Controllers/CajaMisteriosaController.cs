// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(CajaMisteriosaController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(CajaMisteriosaController_class_attributes) ENABLED START*/
[Authorize]
/*PROTECTED REGION END*/
public partial class CajaMisteriosaController : Controller
{
    /*PROTECTED REGION ID(CajaMisteriosaController_dependencies) START*/
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly CajaMisteriosaService _cajaMisteriosaService;
    private readonly ILogger<CajaMisteriosaController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public CajaMisteriosaController(CajaMisteriosaService cajaMisteriosaService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ejemplo.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<CajaMisteriosaController>? logger = null)
    {
        _cajaMisteriosaService = cajaMisteriosaService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<CajaMisteriosaController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(CajaMisteriosaController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_Index_GET_body) START*/

        var entities = await _cajaMisteriosaService.ReadAllAsync();
        return View(entities.Select(CajaMisteriosaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CajaMisteriosaController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_Details_GET_body) START*/

        var entity = await _cajaMisteriosaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(CajaMisteriosaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CajaMisteriosaController_Create_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_Create_GET_body) START*/

        var model = new Skinlink.Web.ViewModels.CajaMisteriosaViewModel();
        model.Probabilidades.Add(new());
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CajaMisteriosaController_Create_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.CajaMisteriosaViewModel model)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_Create_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new CajaMisteriosaService(unitOfWork);

        model.Probabilidades ??= new();
        if (model.Probabilidades.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);

        if (model.Probabilidades is null) { model.Probabilidades = new(); ModelState.AddModelError(nameof(model.Probabilidades), "Debes indicar las líneas."); }
        if (model.Probabilidades.Count < 1 || model.Probabilidades.Count > 500)
            ModelState.AddModelError(nameof(model.Probabilidades), "Número de líneas no válido (máximo 500).");

        for (var index = 0; index < model.Probabilidades.Count; index++)
        {
            var row = model.Probabilidades[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!))
                ModelState.AddModelError($"Probabilidades[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.ActivoRecompensaId is int selectedActivoRecompensa &&
            !row.ActivoRecompensaOptions.Any(option => option.Value == Convert.ToString(selectedActivoRecompensa, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Probabilidades[{index}].ActivoRecompensaId", "Selecciona un registro existente de ActivoDigital.");
        if (row.ActivoRecompensaId is null) ModelState.AddModelError($"Probabilidades[{index}].ActivoRecompensaId", "Debes seleccionar una opción.");
        }

        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.CajaMisteriosa.Create");
        try
        {
            var submittedProbabilidades = new List<ejemplo.Domain.Entities.ProbabilidadDrop>();
            foreach (var row in model.Probabilidades)
            {
                var child = ProbabilidadDropAssembler.Create(row);
                ProbabilidadDropAssembler.ApplyToEntity(row, child);
            child.ActivoRecompensa = row.ActivoRecompensaId is int referenceActivoRecompensa
                ? await unitOfWork.ActivoDigitalRepository.ReadAsync(referenceActivoRecompensa) ?? throw new InvalidOperationException("El registro seleccionado de ActivoDigital ya no existe.") : null!;
                submittedProbabilidades.Add(child);
            }
            var entity = await service.NewAsync(model.Nombre, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioAperturaValor, Divisa = model.PrecioAperturaDivisa }, model.Activa, submittedProbabilidades);

            
            for (var index = 0; index < submittedProbabilidades.Count; index++)
            {
                var child = submittedProbabilidades[index];
                child.Caja = entity;
                if (!entity.Probabilidades.Contains(child)) entity.Probabilidades.Add(child);
            }
            await service.ModifierCajaMisteriosaDefaultAsync(entity);

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
    /*PROTECTED REGION ID(CajaMisteriosaController_Edit_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_Edit_GET_body) START*/

        var entity = await _cajaMisteriosaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = CajaMisteriosaAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CajaMisteriosaController_Edit_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.CajaMisteriosaViewModel model)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new CajaMisteriosaService(unitOfWork);
        var entity = await service.ReadOIDCajaMisteriosaDefaultAsync(id);
        if (entity is null) return NotFound();
        model.Probabilidades ??= new();
        if (model.Probabilidades.Count > 500) return BadRequest("Número de líneas no válido (máximo 500).");
        await LoadFormOptionsAsync(model, unitOfWork);

        if (model.Probabilidades is null) { model.Probabilidades = new(); ModelState.AddModelError(nameof(model.Probabilidades), "Debes indicar las líneas."); }
        if (model.Probabilidades.Count < 1 || model.Probabilidades.Count > 500)
            ModelState.AddModelError(nameof(model.Probabilidades), "Número de líneas no válido (máximo 500).");
        var seenProbabilidades = new HashSet<int>();
        for (var index = 0; index < model.Probabilidades.Count; index++)
        {
            var row = model.Probabilidades[index];
            if (!EqualityComparer<int>.Default.Equals(row.Id, default!) &&
                (!entity.Probabilidades.Any(child => child.Id.Equals(row.Id)) || !seenProbabilidades.Add(row.Id)))
                ModelState.AddModelError($"Probabilidades[{index}].Id", "La línea no pertenece a este registro o está repetida.");
        if (row.ActivoRecompensaId is int selectedActivoRecompensa &&
            !row.ActivoRecompensaOptions.Any(option => option.Value == Convert.ToString(selectedActivoRecompensa, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError($"Probabilidades[{index}].ActivoRecompensaId", "Selecciona un registro existente de ActivoDigital.");
        if (row.ActivoRecompensaId is null) ModelState.AddModelError($"Probabilidades[{index}].ActivoRecompensaId", "Debes seleccionar una opción.");
        }

        if (!ModelState.IsValid) return View(model);

        await unitOfWork.BeginTransactionAsync("MVC.CajaMisteriosa.Edit");
        try
        {
            var submittedProbabilidades = new List<ejemplo.Domain.Entities.ProbabilidadDrop>();
            foreach (var row in model.Probabilidades)
            {
                var child = entity.Probabilidades.SingleOrDefault(item => item.Id.Equals(row.Id)) ?? ProbabilidadDropAssembler.Create(row);
                ProbabilidadDropAssembler.ApplyToEntity(row, child);
            child.ActivoRecompensa = row.ActivoRecompensaId is int referenceActivoRecompensa
                ? await unitOfWork.ActivoDigitalRepository.ReadAsync(referenceActivoRecompensa) ?? throw new InvalidOperationException("El registro seleccionado de ActivoDigital ya no existe.") : null!;
                submittedProbabilidades.Add(child);
            }
            await service.ModifyAsync(id, model.Nombre, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioAperturaValor, Divisa = model.PrecioAperturaDivisa }, model.Activa, submittedProbabilidades);

            foreach (var removed in entity.Probabilidades.Where(child => !submittedProbabilidades.Contains(child)).ToList()) entity.Probabilidades.Remove(removed);
            for (var index = 0; index < submittedProbabilidades.Count; index++)
            {
                var child = submittedProbabilidades[index];
                child.Caja = entity;
                if (!entity.Probabilidades.Contains(child)) entity.Probabilidades.Add(child);
            }
            await service.ModifierCajaMisteriosaDefaultAsync(entity);

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
    /*PROTECTED REGION ID(CajaMisteriosaController_Delete_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_Delete_GET_body) START*/

        var entity = await _cajaMisteriosaService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(CajaMisteriosaAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(CajaMisteriosaController_DeleteConfirmed_POST_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_DeleteConfirmed_POST_body) START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.CajaMisteriosaViewModel());
        await _cajaMisteriosaService.DestroyAsync(id);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CajaMisteriosaController_FiltrarCajasActivas_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarCajasActivas()
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_FiltrarCajasActivas_GET_body) START*/

        var result = await _cajaMisteriosaService.FiltrarCajasActivasAsync();
        return Ok(result.Select(CajaMisteriosaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(CajaMisteriosaController_FiltrarCajasPorPrecioApertura_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarCajasPorPrecioApertura(double p_precioMinimo, double p_precioMaximo)
    {
    /*PROTECTED REGION ID(CajaMisteriosaController_FiltrarCajasPorPrecioApertura_GET_body) START*/

        var result = await _cajaMisteriosaService.FiltrarCajasPorPrecioAperturaAsync(p_precioMinimo, p_precioMaximo);
        return Ok(result.Select(CajaMisteriosaAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(CajaMisteriosaController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsCajaMisteriosaViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.CajaMisteriosaViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CajaMisteriosaController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsCajaMisteriosaViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.CajaMisteriosaViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
    {
        if (model.Probabilidades.Count > 500) { ModelState.AddModelError(nameof(model.Probabilidades), "Máximo 500 líneas."); return; }
        var options0 = (await unitOfWork.ActivoDigitalRepository.ReadAllAsync()).Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = Convert.ToString(item.Id, System.Globalization.CultureInfo.InvariantCulture), Text = Convert.ToString(item.Nombre, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty }).ToList();

        foreach (var row in model.Probabilidades)
        {
        row.ActivoRecompensaOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
        }
        model.ProbabilidadesTemplate.ActivoRecompensaOptions = options0.Select(item => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = item.Value,
            Text = item.Text
        }).ToList();
    }
    /*PROTECTED REGION END*/

}
