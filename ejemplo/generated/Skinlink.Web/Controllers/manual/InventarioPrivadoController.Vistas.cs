// Fichero manual (no lo genera HARIA Web) — Miembro 4: visibilidad del inventario (cambiarVisibilidadInventario).
using Microsoft.AspNetCore.Mvc;
using ejemplo.Domain.Entities;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class InventarioPrivadoController
{
    /// <summary>Estado público/privado del inventario propio (y, para el administrador, de todos).</summary>
    [HttpGet]
    public async Task<IActionResult> Visibilidad()
    {
        if (User.GetUsuarioId() is not int yo) return Challenge();

        await using var uow = _formUnitOfWorkFactory.Create();
        await uow.UsuarioRepository.ReadAllAsync();
        await uow.ActivoDigitalRepository.ReadAllAsync();
        var inventarios = await uow.InventarioPrivadoRepository.ReadAllAsync();

        var mio = inventarios.FirstOrDefault(i => i.Usuario?.Id == yo);
        if (mio is null) return NotFound();

        var model = AVista(mio);
        if (User.EsAdministrador())
        {
            model.Otros = inventarios
                .Where(i => i.Id != mio.Id)
                .OrderBy(i => i.Usuario?.NombreUsuario, StringComparer.CurrentCultureIgnoreCase)
                .Select(AVista)
                .ToList();
        }
        return View("Visibilidad", model);
    }

    private async Task<IActionResult> CambiarVisibilidadDesdeSesionAsync(int inventarioId)
    {
        if (User.GetUsuarioId() is not int yo) return Challenge();

        await using (var uow = _formUnitOfWorkFactory.Create())
        {
            await uow.UsuarioRepository.ReadAllAsync();
            var inventario = (await uow.InventarioPrivadoRepository.ReadAllAsync()).FirstOrDefault(i => i.Id == inventarioId);
            if (inventario is null) return NotFound();
            if (inventario.Usuario?.Id != yo && !User.EsAdministrador()) return Forbid();
        }

        try
        {
            var esPublico = await _inventarioPrivadoService.CambiarVisibilidadInventarioAsync(inventarioId);
            TempData["Exito"] = esPublico
                ? "El inventario ahora es público: otros usuarios pueden verlo."
                : "El inventario ahora es privado: solo lo ve su propietario.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(ex);
        }
        return RedirectToAction(nameof(Visibilidad));
    }

    private static VisibilidadInventarioViewModel AVista(InventarioPrivado inventario) => new()
    {
        InventarioId = inventario.Id,
        Propietario = inventario.Usuario?.NombreUsuario ?? "?",
        EsPublico = inventario.EsPublico,
        NumeroActivos = inventario.Activos.Count,
        ValorEstimado = inventario.Activos.Sum(a => a.PrecioEstimado?.Valor ?? 0),
        Divisa = "EUR"
    };
}
