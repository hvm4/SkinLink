// Fichero manual (no lo genera HARIA Web) — Miembro 4: ciclo de vida de un Intercambio.
// iniciarIntercambio, aceptarIntercambio y cancelarIntercambio (IntercambioController.Manual.cs)
// delegan en estos métodos desde sus regiones protegidas. El usuario que actúa sale siempre de la sesión.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class IntercambioController
{
    /// <summary>Intercambios del usuario: recibidos y enviados pendientes, e historial.</summary>
    [HttpGet]
    public async Task<IActionResult> MisIntercambios()
    {
        if (User.GetUsuarioId() is not int yo) return Challenge();

        await using var uow = _formUnitOfWorkFactory.Create();
        var intercambios = await CargarIntercambiosAsync(uow);

        var mios = intercambios
            .Where(i => i.Proponente?.Id == yo || i.Receptor?.Id == yo)
            .OrderByDescending(i => i.FechaCreacion)
            .Select(i => Resumen(i, yo))
            .ToList();

        var model = new MisIntercambiosViewModel
        {
            Recibidos = mios.Where(r => r.Estado == EstadoIntercambio.Pendiente && r.SoyReceptor).ToList(),
            Enviados = mios.Where(r => r.Estado == EstadoIntercambio.Pendiente && r.SoyProponente).ToList(),
            Historial = mios.Where(r => r.Estado != EstadoIntercambio.Pendiente).ToList()
        };
        return View("MisIntercambios", model);
    }

    /// <summary>
    /// Formulario para proponer un intercambio. Primero se elige el usuario; después se marcan
    /// los activos propios que se ofrecen y los suyos que se piden.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Proponer(int? receptorId)
    {
        if (User.GetUsuarioId() is not int yo) return Challenge();

        await using var uow = _formUnitOfWorkFactory.Create();
        var usuarios = await uow.UsuarioRepository.ReadAllAsync();
        await uow.InventarioPrivadoRepository.ReadAllAsync();
        await uow.IntercambioRepository.ReadAllAsync();
        var activos = await uow.ActivoDigitalRepository.ReadAllAsync();

        var candidatos = usuarios
            .Where(u => u.Id != yo && u.Estado == EstadoCuenta.Activo)
            .OrderBy(u => u.NombreUsuario, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var model = new ProponerIntercambioViewModel
        {
            ReceptorId = receptorId,
            Receptores = candidatos
                .Select(u => new SelectListItem(u.NombreUsuario, u.Id.ToString(), u.Id == receptorId))
                .ToList(),
            MisActivos = ActivosIntercambiables(activos, yo)
        };

        if (receptorId is int id)
        {
            var receptor = candidatos.FirstOrDefault(u => u.Id == id);
            if (receptor is null)
            {
                TempData["Error"] = "No puedes proponer un intercambio a ese usuario.";
                model.ReceptorId = null;
            }
            else
            {
                model.ReceptorNombre = receptor.NombreUsuario;
                model.ActivosReceptor = ActivosIntercambiables(activos, id);
            }
        }

        return View("Proponer", model);
    }

    private async Task<IActionResult> IniciarIntercambioDesdeSesionAsync(
        int receptorId, IReadOnlyList<int>? ofrecidos, IReadOnlyList<int>? solicitados)
    {
        if (User.GetUsuarioId() is not int yo) return Challenge();

        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var useCase = new ejemplo.Application.UseCases.Intercambio.IntercambioUseCase(unitOfWork);
            var intercambio = await useCase.IniciarIntercambioAsync(
                yo, receptorId, ofrecidos ?? Array.Empty<int>(), solicitados ?? Array.Empty<int>());
            TempData["Exito"] = $"Propuesta de intercambio #{intercambio.Id} enviada.";
            return RedirectToAction(nameof(MisIntercambios));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(ex);
            return RedirectToAction(nameof(Proponer), new { receptorId = receptorId > 0 ? receptorId : (int?)null });
        }
    }

    private async Task<IActionResult> ResolverIntercambioAsync(int intercambioId, bool aceptar)
    {
        if (User.GetUsuarioId() is not int yo) return Challenge();

        try
        {
            await using var unitOfWork = _useCaseUnitOfWorkFactory.Create();
            var useCase = new ejemplo.Application.UseCases.Intercambio.IntercambioUseCase(unitOfWork);
            if (aceptar)
            {
                await useCase.AceptarIntercambioAsync(intercambioId, yo);
                TempData["Exito"] = $"Intercambio #{intercambioId} aceptado: los activos ya están en vuestros inventarios.";
            }
            else
            {
                await useCase.CancelarIntercambioAsync(intercambioId, yo);
                // El receptor lo rechaza y el proponente lo cancela: el aviso usa el mismo estado que el historial.
                await using var lectura = _formUnitOfWorkFactory.Create();
                var estado = (await lectura.IntercambioRepository.ReadAsync(intercambioId))?.Estado;
                TempData["Exito"] = estado == ejemplo.Domain.Enums.EstadoIntercambio.Rechazado
                    ? $"Has rechazado el intercambio #{intercambioId}."
                    : $"Has cancelado la propuesta de intercambio #{intercambioId}.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            TempData["Error"] = this.MensajeDeError(ex);
        }

        return RedirectToAction(nameof(MisIntercambios));
    }

    /// <summary>Carga los intercambios con sus usuarios y activos en el mismo contexto.</summary>
    private static async Task<IReadOnlyList<Intercambio>> CargarIntercambiosAsync(IUnitOfWork uow)
    {
        await uow.UsuarioRepository.ReadAllAsync();
        await uow.InventarioPrivadoRepository.ReadAllAsync();
        var intercambios = await uow.IntercambioRepository.ReadAllAsync();
        await uow.ActivoDigitalRepository.ReadAllAsync();
        return intercambios;
    }

    /// <summary>Activos de un usuario que puede ofrecer o que se le pueden pedir.</summary>
    private static List<ActivoIntercambioViewModel> ActivosIntercambiables(IEnumerable<ActivoDigital> activos, int usuarioId) =>
        activos
            .Where(a => a.Inventario?.Usuario?.Id == usuarioId
                        && !a.EnVenta
                        && a.IntercambioOfrecido is null
                        && a.IntercambioSolicitado is null)
            .OrderBy(a => a.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .Select(ActivoParaVista)
            .ToList();

    private static ActivoIntercambioViewModel ActivoParaVista(ActivoDigital a) => new()
    {
        Id = a.Id,
        Nombre = a.Nombre,
        Coleccion = a.Coleccion,
        Rareza = a.Atributos?.Rareza ?? default,
        Condicion = a.Atributos?.Condicion ?? default,
        Precio = a.PrecioEstimado?.Valor ?? 0,
        Divisa = a.PrecioEstimado?.Divisa ?? "EUR"
    };

    private static IntercambioResumenViewModel Resumen(Intercambio i, int yo) => new()
    {
        Id = i.Id,
        Estado = i.Estado,
        FechaCreacion = i.FechaCreacion,
        ProponenteNombre = i.Proponente?.NombreUsuario ?? "?",
        ReceptorNombre = i.Receptor?.NombreUsuario ?? "?",
        Ofrecidos = i.ActivosOfrecidos.Select(ActivoParaVista).ToList(),
        Solicitados = i.ActivosSolicitados.Select(ActivoParaVista).ToList(),
        SoyProponente = i.Proponente?.Id == yo,
        SoyReceptor = i.Receptor?.Id == yo
    };
}
