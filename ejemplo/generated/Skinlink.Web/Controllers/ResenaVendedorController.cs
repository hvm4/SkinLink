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
// Seguridad (Miembro 1): cualquier cuenta activa; las cuentas suspendidas no tienen rol.
// Las acciones con [AllowAnonymous] siguen siendo públicas.
[Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Todos)]
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
    /*PROTECTED REGION ID(ResenaVendedorController_Create_GET_body) ENABLED START*/

        var model = new Skinlink.Web.ViewModels.ResenaVendedorViewModel();
        // RF-18: la reseña la firma siempre el usuario autenticado.
        model.CompradorId = Skinlink.Web.Utils.Security.UsuarioClaimsExtensions.GetUsuarioId(User);
        model.Fecha = DateTime.UtcNow;

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
    /*PROTECTED REGION ID(ResenaVendedorController_Create_POST_body) ENABLED START*/
        // RF-18 / CU-15: el comprador es el usuario autenticado (nunca un valor del formulario),
        // la reseña nace visible y con la fecha actual.
        var compradorActual = Skinlink.Web.Utils.Security.UsuarioClaimsExtensions.GetUsuarioId(User);
        if (compradorActual is null) return Challenge();
        model.CompradorId = compradorActual;
        model.Oculta = false;
        model.Fecha = DateTime.UtcNow;
        ModelState.Remove(nameof(model.CompradorId));
        ModelState.Remove(nameof(model.Oculta));
        ModelState.Remove(nameof(model.Fecha));


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::ejemplo.Application.Services.ResenaVendedorService(unitOfWork);





        await LoadFormOptionsAsync(model, unitOfWork);
        if (model.CompradorId is int selectedComprador &&
            !model.CompradorOptions.Any(option => option.Value == Convert.ToString(selectedComprador, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("CompradorId", "Selecciona un registro existente de Usuario.");
        if (model.CompradorId is null) ModelState.AddModelError("CompradorId", "Debes seleccionar una opción.");
        if (model.VendedorId is int selectedVendedor &&
            !model.VendedorOptions.Any(option => option.Value == Convert.ToString(selectedVendedor, System.Globalization.CultureInfo.InvariantCulture)))
            ModelState.AddModelError("VendedorId", "Selecciona un registro existente de Usuario.");
        if (model.VendedorId is null) ModelState.AddModelError("VendedorId", "Debes seleccionar una opción.");




        // Regla de negocio: solo tras una compra completada al vendedor, una reseña por compra, 1-5 estrellas.
        if (ModelState.IsValid && model.VendedorId is int vendedorElegido)
        {
            try
            {
                await global::ejemplo.Application.Reglas.ReglasResenaVendedor.ValidarNuevaResenaAsync(
                    unitOfWork, compradorActual.Value, vendedorElegido, model.Puntuacion);
            }
            catch (global::ejemplo.Application.Exceptions.ModelException error)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }
        }
        if (!ModelState.IsValid) return View(model);


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.ResenaVendedor.Create");


            var entity = await service.NewAsync(model.Puntuacion, model.Comentario, model.Fecha, model.Oculta, model.CompradorId.GetValueOrDefault(), model.VendedorId.GetValueOrDefault());

            await service.ApplyResenaVendedorRelationsAsync(entity, p_comprador_id: model.CompradorId, p_vendedor_id: model.VendedorId);

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
    /*PROTECTED REGION ID(ResenaVendedorController_Edit_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)] // RF-26: solo el administrador edita o borra reseñas
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
    /*PROTECTED REGION ID(ResenaVendedorController_Edit_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)] // RF-26: solo el administrador edita o borra reseñas
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.ResenaVendedorViewModel model)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_Edit_POST_body) START*/


        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new global::ejemplo.Application.Services.ResenaVendedorService(unitOfWork);

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


        try
        {
            await unitOfWork.BeginTransactionAsync("MVC.ResenaVendedor.Edit");


            await service.ModifyAsync(id, model.Puntuacion, model.Comentario, model.Fecha, model.Oculta);
            entity = await service.ReadOIDResenaVendedorDefaultAsync(id) ?? throw new InvalidOperationException("Entity disappeared during update.");
            await service.ApplyResenaVendedorRelationsAsync(entity, p_comprador_id: model.CompradorId, p_vendedor_id: model.VendedorId);

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
    /*PROTECTED REGION ID(ResenaVendedorController_Delete_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)] // RF-26: solo el administrador edita o borra reseñas
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
    /*PROTECTED REGION ID(ResenaVendedorController_DeleteConfirmed_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)] // RF-26: solo el administrador edita o borra reseñas
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
    /*PROTECTED REGION ID(ResenaVendedorController_FiltrarResenasVisiblesPorVendedor_GET_attributes) ENABLED START*/
    // La reputación de un vendedor es pública (Miembro 4).
    [AllowAnonymous]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarResenasVisiblesPorVendedor(int p_vendedorId)
    {
    /*PROTECTED REGION ID(ResenaVendedorController_FiltrarResenasVisiblesPorVendedor_GET_body) ENABLED START*/
        // Miembro 4: vista HTML en lugar de JSON (ver ResenaVendedorController.Vistas.cs).
        return await MostrarResenasVendedorAsync(p_vendedorId > 0 ? p_vendedorId : null);
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
