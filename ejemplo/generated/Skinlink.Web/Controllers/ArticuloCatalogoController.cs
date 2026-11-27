// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(ArticuloCatalogoController_usings) START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(ArticuloCatalogoController_class_attributes) ENABLED START*/
[Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Todos)]
/*PROTECTED REGION END*/
public partial class ArticuloCatalogoController : Controller
{
    /*PROTECTED REGION ID(ArticuloCatalogoController_dependencies) START*/
    private readonly ArticuloCatalogoService _articuloCatalogoService;
    private readonly ILogger<ArticuloCatalogoController> _logger;
    private readonly Skinlink.Web.Utils.Files.IWebFileStorage _files;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _fileUnitOfWorkFactory;
    private readonly ILogger<ArticuloCatalogoController> _fileLogger;
    public ArticuloCatalogoController(ArticuloCatalogoService articuloCatalogoService, Skinlink.Web.Utils.Files.IWebFileStorage files, ejemplo.Application.Abstractions.IUnitOfWorkFactory fileUnitOfWorkFactory, ILogger<ArticuloCatalogoController> fileLogger)
    {
        _articuloCatalogoService = articuloCatalogoService;
        _files = files;
        _fileUnitOfWorkFactory = fileUnitOfWorkFactory;
        _fileLogger = fileLogger;


        _logger = fileLogger;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Index_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Index_GET_body) START*/

        var entities = await _articuloCatalogoService.ReadAllAsync();
        return View(entities.Select(ArticuloCatalogoAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Details_GET_body) START*/

        var entity = await _articuloCatalogoService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(ArticuloCatalogoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Create_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public IActionResult Create()
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Create_GET_body) START*/

        return View(new Skinlink.Web.ViewModels.ArticuloCatalogoViewModel());
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Create_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.ArticuloCatalogoViewModel model)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Create_POST_body) START*/


        if (model.PorcentajeDescuento is null) ModelState.AddModelError(nameof(model.PorcentajeDescuento), "Debes indicar PorcentajeDescuento.");
        if (model.FechaFinDescuento is null) ModelState.AddModelError(nameof(model.FechaFinDescuento), "Debes indicar FechaFinDescuento.");
        model.Imagen = null;

        var cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;
        var newFiles = new List<string>();
        var commitStarted = false;


        await using var unitOfWork = _fileUnitOfWorkFactory.Create();
        var fileService = new ArticuloCatalogoService(unitOfWork);
        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.ArticuloCatalogo.Create");

            await ValidateUploadsAsync(model, cancellationToken);
            if (!ModelState.IsValid) { await unitOfWork.RollbackAsync(); return View(model); }
            var entity = await fileService.NewAsync(model.Nombre, model.Coleccion, model.Rareza, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioReferenciaValor, Divisa = model.PrecioReferenciaDivisa }, model.Habilitado, model.PorcentajeDescuento.GetValueOrDefault(), model.FechaFinDescuento.GetValueOrDefault(), string.Empty);
            if (true && model.ImagenUpload is not null)
            {
                var stored = await _files.SaveAsync(model.ImagenUpload, "ArticuloCatalogo", "Imagen", entity.Id, cancellationToken);
                newFiles.Add(stored);
                entity.Imagen = stored;
            }
            if (newFiles.Count > 0) await fileService.ModifierArticuloCatalogoDefaultAsync(entity);

            commitStarted = true;
            await unitOfWork.CommitAsync();
        }
        catch (Exception error)
        {
            // A commit exception can mean that SQL committed but its acknowledgement
            // was lost. Retain the new files until the outcome can be reconciled.
            var rolledBack = !commitStarted;
            try { await unitOfWork.RollbackAsync(); }
            catch (Exception rollbackError) { rolledBack = false; _fileLogger.LogError(rollbackError, "Upload database rollback failed; uploaded files retained for reconciliation."); }
            if (commitStarted) _fileLogger.LogWarning(error, "Upload commit outcome is uncertain; uploaded files retained for reconciliation.");
            if (rolledBack) foreach (var stored in newFiles) await _files.DeleteNewAsync(stored);
            if (error is OperationCanceledException) throw;
            _fileLogger.LogError(error, "File update failed for ArticuloCatalogo.");
            ModelState.AddModelError(string.Empty, "No se han podido guardar el registro y el archivo. Inténtalo de nuevo.");
            return View(model);
        }


        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Edit_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Edit_GET_body) START*/

        var entity = await _articuloCatalogoService.ReadOIDArticuloCatalogoDefaultAsync(id);
        if (entity is null) return NotFound();
        return View(ArticuloCatalogoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Edit_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.ArticuloCatalogoViewModel model)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Edit_POST_body) START*/


        if (model.PorcentajeDescuento is null) ModelState.AddModelError(nameof(model.PorcentajeDescuento), "Debes indicar PorcentajeDescuento.");
        if (model.FechaFinDescuento is null) ModelState.AddModelError(nameof(model.FechaFinDescuento), "Debes indicar FechaFinDescuento.");


        var cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;
        var newFiles = new List<string>();
        var commitStarted = false;

        string? previousImagen = null;
        await using var unitOfWork = _fileUnitOfWorkFactory.Create();
        var fileService = new ArticuloCatalogoService(unitOfWork);
        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.ArticuloCatalogo.Edit");
            var entity = await fileService.ReadOIDArticuloCatalogoDefaultAsync(id);
            if (entity is null) { await unitOfWork.RollbackAsync(); return NotFound(); }
            previousImagen = entity.Imagen;
            model.Imagen = previousImagen;
            await ValidateUploadsAsync(model, cancellationToken);
            if (!ModelState.IsValid) { await unitOfWork.RollbackAsync(); return View(model); }
            await fileService.ModifyAsync(id, model.Nombre, model.Coleccion, model.Rareza, new global::ejemplo.Domain.ValueObjects.Moneda { Valor = model.PrecioReferenciaValor, Divisa = model.PrecioReferenciaDivisa }, model.Habilitado, model.PorcentajeDescuento.GetValueOrDefault(), model.FechaFinDescuento.GetValueOrDefault(), previousImagen!);
            entity = await fileService.ReadOIDArticuloCatalogoDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            if (true && model.ImagenUpload is not null)
            {
                var stored = await _files.SaveAsync(model.ImagenUpload, "ArticuloCatalogo", "Imagen", entity.Id, cancellationToken);
                newFiles.Add(stored);
                entity.Imagen = stored;
            }
            if (newFiles.Count > 0) await fileService.ModifierArticuloCatalogoDefaultAsync(entity);

            commitStarted = true;
            await unitOfWork.CommitAsync();
        }
        catch (Exception error)
        {
            // A commit exception can mean that SQL committed but its acknowledgement
            // was lost. Retain the new files until the outcome can be reconciled.
            var rolledBack = !commitStarted;
            try { await unitOfWork.RollbackAsync(); }
            catch (Exception rollbackError) { rolledBack = false; _fileLogger.LogError(rollbackError, "Upload database rollback failed; uploaded files retained for reconciliation."); }
            if (commitStarted) _fileLogger.LogWarning(error, "Upload commit outcome is uncertain; uploaded files retained for reconciliation.");
            if (rolledBack) foreach (var stored in newFiles) await _files.DeleteNewAsync(stored);
            if (error is OperationCanceledException) throw;
            _fileLogger.LogError(error, "File update failed for ArticuloCatalogo.");
            ModelState.AddModelError(string.Empty, "No se han podido guardar el registro y el archivo. Inténtalo de nuevo.");
            return View(model);
        }
            if (model.ImagenUpload is not null && true) await _files.DeleteReplacedAsync(previousImagen);

        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(ArticuloCatalogoController_Delete_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_Delete_GET_body) START*/

        var entity = await _articuloCatalogoService.ReadOIDArticuloCatalogoDefaultAsync(id);
        if (entity is null) return NotFound();
        return View(ArticuloCatalogoAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(ArticuloCatalogoController_DeleteConfirmed_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(ArticuloCatalogoController_DeleteConfirmed_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.ArticuloCatalogoViewModel());
        await using var unitOfWork = _fileUnitOfWorkFactory.Create();
        var fileService = new ArticuloCatalogoService(unitOfWork);
        await unitOfWork.BeginTransactionAsync("MVC.ArticuloCatalogo.Delete");
        var entity = await fileService.ReadOIDArticuloCatalogoDefaultAsync(id);
        if (entity is null) { await unitOfWork.RollbackAsync(); return NotFound(); }
        try
        {
            await fileService.DestroyAsync(id);
            await unitOfWork.CommitAsync();
        }
        catch (Exception error)
        {
            await unitOfWork.RollbackAsync();
            _fileLogger.LogError(error, "Could not delete ArticuloCatalogo.");
            ModelState.AddModelError(string.Empty, "No se ha podido eliminar el registro.");
            TempData["ErrorBorrado"] = this.MensajeDeErrorAlEliminar(error);
            return View("Delete", ArticuloCatalogoAssembler.ToViewModel(entity));
        }
        await _files.DeleteEntityFileAsync(entity.Imagen);
        return RedirectToAction(nameof(Index));
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(ArticuloCatalogoController_method_ValidateUploadsAsync) START*/
    private async Task ValidateUploadsAsync(Skinlink.Web.ViewModels.ArticuloCatalogoViewModel model, CancellationToken cancellationToken)
    {
        if (true)
        {
            if (model.ImagenUpload is not null)
            {
                try { await _files.ValidateAsync(model.ImagenUpload, cancellationToken); }
                catch (InvalidDataException error) { ModelState.AddModelError(nameof(model.ImagenUpload), error.Message); }
            }
        }
    }
    /*PROTECTED REGION END*/
}
