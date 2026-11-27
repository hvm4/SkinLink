// HARIA-WEB GENERATED sha256:dcae23faa6fbe41d5727847b469d73021add38d40d755bd7bfde8213251b9ab3
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skinlink.Web.Assemblers;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly ejemplo.Application.Services.UsuarioService _UsuarioService;
    private readonly ejemplo.Application.Abstractions.IUnitOfWorkFactory _factory;
    public AccountController(ejemplo.Application.Services.UsuarioService UsuarioService, ejemplo.Application.Abstractions.IUnitOfWorkFactory factory)
    {
        _UsuarioService = UsuarioService;
        _factory = factory;
    }
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ModelState.Remove(nameof(model.Id));
        if (!ModelState.IsValid) return View(model);
        var rawIdentifier = model.Identifier ?? Convert.ToString(model.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(rawIdentifier))
        {
            ModelState.AddModelError(nameof(model.Identifier), "Debes indicar el identificador de acceso.");
            return View(model);
        }
        switch (model.UserClass)
        {
            case "Usuario":
            {

                var token = await _UsuarioService.LoginAsync(rawIdentifier, model.Password);
                if (string.IsNullOrWhiteSpace(token)) break;
                await using var unit = _factory.Create();
                var matches = (await unit.UsuarioRepository.ReadAllAsync()).Where(item => item.Correo == rawIdentifier).Take(2).ToList();
                var entity = matches.Count == 1 ? matches[0] : null;
                if (entity is null) break;
                await HariaWebAuthentication.SignInAsync(HttpContext, "Usuario", entity.Id, entity);
                HttpContext.Session.SetUser(UsuarioAssembler.ToViewModel(entity));
                return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl!) : RedirectToAction("Index", "Home");
            }
        }
        ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
        return View(model);
    }
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.ClearClasses();
        return RedirectToAction(nameof(Login));
    }
    [AllowAnonymous, HttpGet]
    public IActionResult AccessDenied() => View();
}
