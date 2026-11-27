// Fichero manual (no lo genera HARIA Web) — Miembro 4: vistas de las reseñas de vendedores.
// La acción generada FiltrarResenasVisiblesPorVendedor y las operaciones OcultarResena / ModerarResena
// delegan en estos métodos desde sus regiones protegidas.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ejemplo.Application.Services;
using Skinlink.Web.Assemblers;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class ResenaVendedorController
{
    /// <summary>Panel de moderación: todas las reseñas (visibles y ocultas) para el administrador.</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Moderacion()
    {
        await using var uow = _formUnitOfWorkFactory.Create();
        await uow.UsuarioRepository.ReadAllAsync();
        var resenas = await uow.ResenaVendedorRepository.ReadAllAsync();

        var model = new ModeracionResenasViewModel
        {
            Resenas = resenas
                .OrderByDescending(r => r.Fecha)
                .Select(ResenaVendedorAssembler.ToViewModel)
                .ToList()
        };
        return View("Moderacion", model);
    }

    /// <summary>
    /// filtrarResenasVisiblesPorVendedor en HTML: reseñas no ocultas de un vendedor y su nota media.
    /// Si no se indica vendedor, muestra solo el selector.
    /// </summary>
    private async Task<IActionResult> MostrarResenasVendedorAsync(int? vendedorId)
    {
        await using var uow = _formUnitOfWorkFactory.Create();
        var usuarios = await uow.UsuarioRepository.ReadAllAsync();

        var model = new ResenasVendedorPaginaViewModel
        {
            VendedorId = vendedorId,
            EsAdministrador = User.EsAdministrador(),
            Vendedores = usuarios
                .OrderBy(u => u.NombreUsuario, StringComparer.CurrentCultureIgnoreCase)
                .Select(u => new SelectListItem(u.NombreUsuario, u.Id.ToString(), u.Id == vendedorId))
                .ToList()
        };

        if (vendedorId is int id)
        {
            var vendedor = usuarios.FirstOrDefault(u => u.Id == id);
            if (vendedor is null)
            {
                TempData["Error"] = "Ese vendedor no existe.";
                model.VendedorId = null;
                return View("FiltrarResenasVisiblesPorVendedor", model);
            }

            // Mismo UnitOfWork que los usuarios: EF enlaza comprador y vendedor de cada reseña.
            var visibles = await new ResenaVendedorService(uow).FiltrarResenasVisiblesPorVendedorAsync(id);
            model.VendedorNombre = vendedor.NombreUsuario;
            model.Resenas = visibles
                .OrderByDescending(r => r.Fecha)
                .Select(ResenaVendedorAssembler.ToViewModel)
                .ToList();
            model.NotaMedia = model.Resenas.Count > 0 ? model.Resenas.Average(r => r.Puntuacion) : null;
            model.PuedeOcultar = User.EsAdministrador() || User.GetUsuarioId() == id;
        }

        return View("FiltrarResenasVisiblesPorVendedor", model);
    }

    /// <summary>ocultarResena: solo el vendedor reseñado o un administrador.</summary>
    private async Task<IActionResult> OcultarResenaDeMiPerfilAsync(int resenaId)
    {
        if (User.GetUsuarioId() is not int usuarioId) return Challenge();

        await using var uow = _formUnitOfWorkFactory.Create();
        await uow.UsuarioRepository.ReadAllAsync();
        var resena = (await uow.ResenaVendedorRepository.ReadAllAsync()).FirstOrDefault(r => r.Id == resenaId);
        if (resena is null) return NotFound();

        var vendedorId = resena.Vendedor?.Id;
        if (vendedorId != usuarioId && !User.EsAdministrador()) return Forbid();

        try
        {
            await new ResenaVendedorService(uow).OcultarResenaAsync(resenaId);
            TempData["Exito"] = "La reseña ya no se muestra en el perfil.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(ex);
        }

        return RedirectToAction(nameof(FiltrarResenasVisiblesPorVendedor), new { p_vendedorId = vendedorId });
    }
}
