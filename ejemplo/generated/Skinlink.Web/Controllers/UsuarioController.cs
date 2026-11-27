// HARIA-WEB MVC managed controller
/*PROTECTED REGION ID(UsuarioController_usings) ENABLED START*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ejemplo.Domain.Entities;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
/*PROTECTED REGION END*/

namespace Skinlink.Web.Controllers;

/*PROTECTED REGION ID(UsuarioController_class_attributes) ENABLED START*/
// Seguridad (Miembro 1): cualquier cuenta activa; las cuentas suspendidas no tienen rol.
// Las acciones con [AllowAnonymous] siguen siendo públicas.
[Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Todos)]
/*PROTECTED REGION END*/
public partial class UsuarioController : Controller
{
    /*PROTECTED REGION ID(UsuarioController_dependencies) START*/
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _useCaseUnitOfWorkFactory;
    private readonly UsuarioService _usuarioService;
    private readonly ILogger<UsuarioController> _logger;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _formUnitOfWorkFactory;
    public UsuarioController(UsuarioService usuarioService, ejemplo.Application.Abstractions.IUnitOfWorkFactory formUnitOfWorkFactory, ejemplo.Application.Abstractions.IUnitOfWorkFactory useCaseUnitOfWorkFactory, ILogger<UsuarioController>? logger = null)
    {
        _usuarioService = usuarioService;

        _formUnitOfWorkFactory = formUnitOfWorkFactory;
        _useCaseUnitOfWorkFactory = useCaseUnitOfWorkFactory;
        _logger = logger ?? global::Microsoft.Extensions.Logging.Abstractions.NullLogger<UsuarioController>.Instance;
    }
    /*PROTECTED REGION END*/
    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Index_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Index()
    {
    /*PROTECTED REGION ID(UsuarioController_Index_GET_body) ENABLED START*/

        var entities = await _usuarioService.ReadAllAsync();
        // Herencia Usuario <- Administrador: el listado incluye los dos tipos y marca los administradores.
        ViewData["AdministradorIds"] = entities.OfType<Administrador>().Select(item => item.Id).ToHashSet();
        return View("Listado", entities.Select(UsuarioAssembler.ToViewModel).ToList());
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Details_GET_attributes) START*/
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Details(int id)
    {
    /*PROTECTED REGION ID(UsuarioController_Details_GET_body) ENABLED START*/

        // Un usuario solo puede ver su propia ficha; un administrador, cualquiera.
        if (!User.PuedeAccederAUsuario(id)) return Forbid();
        var entity = await _usuarioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View("Perfil", ConstruirPerfil(entity));
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Create_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create()
    {
    /*PROTECTED REGION ID(UsuarioController_Create_GET_body) ENABLED START*/

        await Task.CompletedTask; // la acción generada es async, pero este cuerpo no consulta nada (evita el aviso CS1998)

        // Alta administrativa: el monedero y el inventario se crean vacíos automáticamente.
        return View("Alta", new UsuarioViewModel
        {
            FechaNacimiento = DateTime.Today.AddYears(-18),
            FechaUltimoLogin = DateTime.Today,
            Estado = EstadoCuenta.Activo,
            Nivel = 1,
            Idioma = "es"
        });
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_Create_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Create(Skinlink.Web.ViewModels.UsuarioViewModel model)
    {
    /*PROTECTED REGION ID(UsuarioController_Create_POST_body) ENABLED START*/

        // Alta administrativa: cada Usuario tiene su propio Monedero e InventarioPrivado (relaciones 1 a 1),
        // así que se crean nuevos en lugar de elegir uno existente que ya pertenece a otro usuario.
        QuitarCamposGestionadosPorServidor();
        if (string.IsNullOrEmpty(model.Contrasena)) ModelState.AddModelError(nameof(model.Contrasena), "Debes indicar una contraseña.");
        if (await CorreoEnUsoAsync(model.Correo)) ModelState.AddModelError(nameof(model.Correo), "Ya existe una cuenta con ese correo.");
        if (!ModelState.IsValid) return View("Alta", model);

        try
        {
            var entity = await _usuarioService.NewAsync(model.NombreUsuario, model.Correo, model.Contrasena!, model.FechaNacimiento, model.Estado, model.Nivel, model.Experiencia, model.FechaUltimoLogin, model.TemaModoOscuro, model.Idioma, model.Verificado, CuentaNueva.Monedero(), CuentaNueva.Inventario());
            TempData["Exito"] = $"Usuario creado. Puede iniciar sesión con su correo ({entity.Correo}).";
            return RedirectToAction(nameof(Details), new { id = entity.Id });
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "No se ha podido crear el usuario.");
            ModelState.AddModelError(string.Empty, this.MensajeDeError(error));
            return View("Alta", model);
        }
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Register_GET_attributes) START*/
    [AllowAnonymous]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Register()
    {
    /*PROTECTED REGION ID(UsuarioController_Register_GET_body) ENABLED START*/

        await Task.CompletedTask; // la acción generada es async, pero este cuerpo no consulta nada (evita el aviso CS1998)

        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Perfil));
        return View("Registro", new UsuarioViewModel { FechaNacimiento = DateTime.Today.AddYears(-18), Idioma = "es" });
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_Register_POST_attributes) START*/
    [AllowAnonymous]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Register(Skinlink.Web.ViewModels.UsuarioViewModel model)
    {
    /*PROTECTED REGION ID(UsuarioController_Register_POST_body) ENABLED START*/

        // Registro propio: el usuario solo elige sus datos personales. El estado, el nivel, la experiencia
        // y la verificación los fija el servidor, y el monedero y el inventario se crean vacíos.
        QuitarCamposDelRegistro();
        if (string.IsNullOrEmpty(model.Contrasena)) ModelState.AddModelError(nameof(model.Contrasena), "Debes indicar una contraseña.");
        if (model.FechaNacimiento.Date > DateTime.Today) ModelState.AddModelError(nameof(model.FechaNacimiento), "La fecha de nacimiento no puede ser futura.");
        if (await CorreoEnUsoAsync(model.Correo)) ModelState.AddModelError(nameof(model.Correo), "Ya existe una cuenta con ese correo.");
        if (!ModelState.IsValid) return View("Registro", model);

        Usuario createdUser;
        try
        {
            createdUser = await _usuarioService.NewAsync(model.NombreUsuario, model.Correo, model.Contrasena!, model.FechaNacimiento,
                EstadoCuenta.Activo, 1, 0, DateTime.UtcNow, model.TemaModoOscuro, model.Idioma, false,
                CuentaNueva.Monedero(), CuentaNueva.Inventario());
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "No se ha podido registrar el usuario.");
            ModelState.AddModelError(string.Empty, this.MensajeDeError(error));
            return View("Registro", model);
        }
        try
        {
            await HariaWebAuthentication.SignInAsync(HttpContext, "Usuario", createdUser.Id, createdUser);
            HttpContext.Session.SetUser(UsuarioAssembler.ToViewModel(createdUser));
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogError(error, "La cuenta se ha creado, pero no se ha podido iniciar la sesión.");
            TempData["Exito"] = $"Tu cuenta ya está creada. Inicia sesión con tu correo ({createdUser.Correo}) y tu contraseña.";
            return RedirectToAction("Login", "Account");
        }
        TempData["Exito"] = $"¡Bienvenido a SkinLink! A partir de ahora inicia sesión con tu correo ({createdUser.Correo}).";
        return RedirectToAction(nameof(Details), new { id = createdUser.Id });
    /*PROTECTED REGION END*/
    }

    [HttpGet]
    /*PROTECTED REGION ID(UsuarioController_Edit_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id)
    {
    /*PROTECTED REGION ID(UsuarioController_Edit_GET_body) START*/

        var entity = await _usuarioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        var model = UsuarioAssembler.ToViewModel(entity);
        await LoadFormOptionsAsync(model);
        return View(model);
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_Edit_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Edit(int id, Skinlink.Web.ViewModels.UsuarioViewModel model)
    {
    /*PROTECTED REGION ID(UsuarioController_Edit_POST_body) ENABLED START*/

        ModelState.Remove(nameof(model.FechaUltimaRecompensa)); // Campo fuera del contrato Edit de Application.
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var service = new UsuarioService(unitOfWork);
        var entity = await service.ReadOIDUsuarioDefaultAsync(id);
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

        await unitOfWork.BeginTransactionAsync("MVC.Usuario.Edit");
        try
        {

            await service.ModifyAsync(id, model.NombreUsuario, model.Correo, (string.IsNullOrEmpty(model.Contrasena) ? entity.Contrasena : model.Contrasena), model.FechaNacimiento, model.Estado, model.Nivel, model.Experiencia, model.FechaUltimoLogin, model.TemaModoOscuro, model.Idioma, model.Verificado, null!, null!);
            entity.Monedero = model.MonederoId is int referenceMonedero
                ? await unitOfWork.MonederoRepository.ReadAsync(referenceMonedero) ?? throw new InvalidOperationException("El registro seleccionado de Monedero ya no existe.") : null!;
            entity.Inventario = model.InventarioId is int referenceInventario
                ? await unitOfWork.InventarioPrivadoRepository.ReadAsync(referenceInventario) ?? throw new InvalidOperationException("El registro seleccionado de InventarioPrivado ya no existe.") : null!;

            await service.ModifierUsuarioDefaultAsync(entity);

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
    /*PROTECTED REGION ID(UsuarioController_Delete_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> Delete(int id)
    {
    /*PROTECTED REGION ID(UsuarioController_Delete_GET_body) START*/

        var entity = await _usuarioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(UsuarioAssembler.ToViewModel(entity));
    /*PROTECTED REGION END*/
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    /*PROTECTED REGION ID(UsuarioController_DeleteConfirmed_POST_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
    /*PROTECTED REGION ID(UsuarioController_DeleteConfirmed_POST_body) ENABLED START*/

        if (!ModelState.IsValid) return View("Delete", new Skinlink.Web.ViewModels.UsuarioViewModel());
        // El administrador no puede borrar su propia cuenta (se quedaría sin acceso y sin administrador).
        if (Skinlink.Web.Utils.Security.UsuarioClaimsExtensions.GetUsuarioId(User) == id)
        {
            TempData["ErrorBorrado"] = "No puedes eliminar tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            await _usuarioService.DestroyAsync(id);
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
    /*PROTECTED REGION ID(UsuarioController_FiltrarUsuariosPorEstado_GET_attributes) ENABLED START*/
    [Authorize(Roles = Skinlink.Web.Utils.Security.Roles.Administrador)]
    /*PROTECTED REGION END*/
    public async Task<IActionResult> FiltrarUsuariosPorEstado(EstadoCuenta p_estado)
    {
    /*PROTECTED REGION ID(UsuarioController_FiltrarUsuariosPorEstado_GET_body) ENABLED START*/

        // ReadFilter filtrarUsuariosPorEstado (filterLinQ: entity => entity.Estado == p_estado).
        // Sin parámetro en la URL solo se muestra el formulario.
        var model = new UsuarioFiltroEstadoViewModel();
        if (Request.Query.ContainsKey("p_estado") && Enum.IsDefined(p_estado))
        {
            var result = await _usuarioService.FiltrarUsuariosPorEstadoAsync(p_estado);
            model.Estado = p_estado;
            model.Buscado = true;
            model.Resultados = result.Select(UsuarioAssembler.ToViewModel).ToList();
            model.AdministradorIds = result.OfType<Administrador>().Select(item => item.Id).ToHashSet();
        }
        return View(model);
    /*PROTECTED REGION END*/
    }

    /*PROTECTED REGION ID(UsuarioController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsUsuarioViewModel) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.UsuarioViewModel model)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await LoadFormOptionsAsync(model, unitOfWork);
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(UsuarioController_method_LoadFormOptionsAsync_SkinlinkWebViewModelsUsuarioViewModel_ejemploApplicationAbstractionsIUnitOfWork) START*/
    private async Task LoadFormOptionsAsync(Skinlink.Web.ViewModels.UsuarioViewModel model, ejemplo.Application.Abstractions.IUnitOfWork unitOfWork)
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