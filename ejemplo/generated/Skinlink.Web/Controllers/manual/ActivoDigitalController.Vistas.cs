// Fichero manual (no lo genera HARIA Web): vistas de los filtros y operaciones de ActivoDigital.
// Las acciones generadas (FiltrarActivosPor..., BloquearParaVenta, ...) delegan en estos métodos
// desde sus regiones protegidas.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ejemplo.Domain.Entities;
using Skinlink.Web.Assemblers;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class ActivoDigitalController
{
    /// <summary>Catálogo con los cuatro filtros (CU-10). Visible sin iniciar sesión.</summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Buscar() => View("Buscar", NuevaBusqueda());

    /// <summary>Panel de operaciones de un activo.</summary>
    [HttpGet]
    public async Task<IActionResult> Operaciones(int id) =>
        await MostrarOperacionesAsync(id, autentico: null, reporte: null);

    private bool EsAdministrador() => User.EsAdministrador();
    private int? UsuarioActualId() => User.GetUsuarioId();

    private ActivoBusquedaViewModel NuevaBusqueda() => new()
    {
        EsAdministrador = User.EsAdministrador(),
        Autenticado = User.Identity?.IsAuthenticated == true,
        UsuarioId = User.GetUsuarioId()
    };

    private IActionResult MostrarBusqueda(ActivoBusquedaViewModel model, string filtro, IEnumerable<ActivoDigital> resultados)
    {
        model.FiltroAplicado = filtro;
        model.Resultados = resultados
            .OrderBy(a => a.Nombre)
            .Select(ActivoDigitalAssembler.ToViewModel)
            .ToList();
        return View("Buscar", model);
    }

    /// <summary>Carga el activo con su inventario y dueño para decidir qué operaciones se muestran.</summary>
    private async Task<(ActivoDigital? activo, int? propietarioId, string? propietario)> CargarActivoConDuenoAsync(int id)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await unitOfWork.InventarioPrivadoRepository.ReadAllAsync(); // incluye Usuario de cada inventario
        var activo = await unitOfWork.ActivoDigitalRepository.ReadAsync(id);
        var usuario = activo?.Inventario?.Usuario;
        return (activo, usuario?.Id, usuario?.NombreUsuario);
    }

    /// <summary>true si el usuario autenticado es el dueño del activo o un administrador.</summary>
    private async Task<bool> PuedeGestionarActivoAsync(int id)
    {
        if (User.EsAdministrador()) return true;
        var (_, propietarioId, _) = await CargarActivoConDuenoAsync(id);
        return propietarioId is not null && propietarioId == User.GetUsuarioId();
    }

    private async Task<IActionResult> MostrarOperacionesAsync(int id, bool? autentico, string? reporte)
    {
        var (activo, propietarioId, propietario) = await CargarActivoConDuenoAsync(id);
        if (activo is null) return NotFound();

        var esAdmin = User.EsAdministrador();
        var esPropietario = propietarioId is not null && propietarioId == User.GetUsuarioId();
        if (!esAdmin && !esPropietario) return Forbid();

        return View("Operaciones", new ActivoOperacionesViewModel
        {
            Activo = ActivoDigitalAssembler.ToViewModel(activo),
            Propietario = propietario,
            EsPropietario = esPropietario,
            EsAdministrador = esAdmin,
            NuevoPrecioValor = activo.PrecioEstimado?.Valor ?? 0,
            NuevoPrecioDivisa = activo.PrecioEstimado?.Divisa ?? "EUR",
            Autentico = autentico,
            Reporte = reporte
        });
    }

    /// <summary>Ejecuta una operación y vuelve al panel con un mensaje de éxito o el error de negocio.</summary>
    private async Task<IActionResult> EjecutarYVolverAsync(int id, Func<Task> operacion, string mensajeExito)
    {
        try
        {
            await operacion();
            TempData["Exito"] = mensajeExito;
        }
        catch (Exception ex)
        {
            TempData["Error"] = MensajesOperacion.De(ex);
        }
        return RedirectToAction(nameof(Operaciones), new { id });
    }
}

/// <summary>Convierte las excepciones de Application en un texto legible para el usuario.</summary>
public static class MensajesOperacion
{
    public static string De(Exception ex)
    {
        // Los casos de uso envuelven la causa real: se muestra el mensaje más interno de negocio.
        var actual = ex;
        while (actual.InnerException is not null && actual is ejemplo.Application.Exceptions.ModelException)
            actual = actual.InnerException;
        return actual is ejemplo.Application.Exceptions.ModelException or InvalidOperationException
            ? actual.Message
            : "No se ha podido completar la operación: " + actual.Message;
    }
}
