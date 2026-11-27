// Fichero manual (no lo genera HARIA Web) — Miembro 4: favoritos del usuario conectado.
// Usa la operación marcarComoFavorito de ActivoDigital (alterna: si ya era favorito, lo quita).
using Microsoft.AspNetCore.Mvc;
using ejemplo.Application.Services;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class FavoritoController
{
    [HttpGet]
    public async Task<IActionResult> MisFavoritos()
    {
        if (User.GetUsuarioId() is not int yo) return Challenge();

        await using var uow = _formUnitOfWorkFactory.Create();
        var favoritos = await uow.FavoritoRepository.ReadAllAsync();

        var model = new MisFavoritosViewModel
        {
            Favoritos = favoritos
                .Where(f => f.Usuario?.Id == yo && f.ActivoDigital is not null)
                .OrderByDescending(f => f.FechaMarcado)
                .Select(f => new FavoritoFilaViewModel
                {
                    FavoritoId = f.Id,
                    ActivoId = f.ActivoDigital.Id,
                    Nombre = f.ActivoDigital.Nombre,
                    Coleccion = f.ActivoDigital.Coleccion,
                    Rareza = f.ActivoDigital.Atributos?.Rareza ?? default,
                    Condicion = f.ActivoDigital.Atributos?.Condicion ?? default,
                    Precio = f.ActivoDigital.PrecioEstimado?.Valor ?? 0,
                    Divisa = f.ActivoDigital.PrecioEstimado?.Divisa ?? "EUR",
                    EnVenta = f.ActivoDigital.EnVenta,
                    FechaMarcado = f.FechaMarcado
                })
                .ToList()
        };
        return View("MisFavoritos", model);
    }

    /// <summary>
    /// Marca o desmarca un activo como favorito del usuario de la sesión.
    /// Desde cualquier vista: form POST a /Favorito/Alternar con activoId (y returnUrl opcional).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alternar(int activoId, string? returnUrl)
    {
        if (User.GetUsuarioId() is not int yo) return Challenge();

        try
        {
            await using var uow = _formUnitOfWorkFactory.Create();
            var ahoraEsFavorito = await new ActivoDigitalService(uow).MarcarComoFavoritoAsync(activoId, yo);
            TempData["Exito"] = ahoraEsFavorito ? "Añadido a tus favoritos." : "Quitado de tus favoritos.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(ex);
        }

        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(MisFavoritos));
    }
}
