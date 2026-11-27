// Fichero manual (no lo genera HARIA Web): "Poner a la venta" para usuarios.
// El alta genérica de Venta (solo administrador) pide comprador y transacciones; aquí el usuario
// elige uno de sus activos y un precio, y se hace en dos pasos con las operaciones del modelo:
// Venta.new (vendedor = usuario de la sesión, estado Disponible) y Venta.publicarVenta.
// El modelo solo admite una venta por activo (relación 1 a 1): una venta cancelada del propio usuario
// se reutiliza (sin volver a dar experiencia); un activo que ya se vendió no se puede volver a vender.
using Microsoft.AspNetCore.Mvc;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using Skinlink.Web.Utils;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class VentaController
{
    [HttpGet]
    public async Task<IActionResult> PonerALaVenta(int? activoId)
    {
        var usuarioId = User.GetUsuarioId();
        if (usuarioId is null) return Challenge();
        return View(await CargarPonerALaVentaAsync(usuarioId.Value, activoId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PonerALaVenta(int p_activo_oid, Moneda p_precio)
    {
        var usuarioId = User.GetUsuarioId();
        if (usuarioId is null) return Challenge();

        int? ventaCreada = null;
        int? ventaReutilizada = null;
        try
        {
            if (p_precio is null || double.IsNaN(p_precio.Valor) || p_precio.Valor <= 0)
                throw new ModelException("Indica un precio de venta mayor que 0.");
            if (string.IsNullOrWhiteSpace(p_precio.Divisa)) p_precio.Divisa = "EUR";
            p_precio.Divisa = p_precio.Divisa.Trim().ToUpperInvariant();

            // 1) Comprobar el activo y dejar preparada una venta Disponible a nombre del usuario.
            int ventaId;
            string nombreActivo;
            await using (var unitOfWork = _formUnitOfWorkFactory.Create())
            {
                await unitOfWork.UsuarioRepository.ReadAllAsync();
                await unitOfWork.InventarioPrivadoRepository.ReadAllAsync();
                await unitOfWork.VentaRepository.ReadAllAsync();
                var activo = await unitOfWork.ActivoDigitalRepository.ReadAsync(p_activo_oid)
                    ?? throw new ModelException("El activo no existe.");
                nombreActivo = activo.Nombre;
                var motivo = MotivoNoVendible(activo, usuarioId.Value);
                if (motivo is not null) throw new ModelException(motivo);

                var servicio = new VentaService(unitOfWork);
                if (activo.Venta is null)
                {
                    var venta = await servicio.NewAsync(p_precio, DateTime.UtcNow, EstadoVenta.Disponible, usuarioId.Value, 0, activo.Id, 0, 0);
                    ventaId = venta.Id;
                    ventaCreada = venta.Id;
                }
                else
                {
                    // Venta cancelada del propio usuario: se vuelve a abrir con el precio nuevo.
                    ventaId = activo.Venta.Id;
                    await servicio.ModifyAsync(ventaId, p_precio, DateTime.UtcNow, EstadoVenta.Disponible, activo.Id);
                    ventaReutilizada = ventaId;
                }
            }

            // 2) Publicarla.
            //    - Venta nueva: operación del modelo publicarVenta (comprueba propiedad, bloqueo e intercambios y da +10 XP).
            //    - Venta cancelada reutilizada: ya se publicó una vez y dio su experiencia; solo se vuelve a bloquear
            //      el activo para la venta (las comprobaciones las ha hecho MotivoNoVendible). Así publicar y
            //      cancelar en bucle no sirve para acumular experiencia.
            await using (var unitOfWork = _formUnitOfWorkFactory.Create())
            {
                if (ventaReutilizada is null)
                    await new VentaService(unitOfWork).PublicarVentaAsync(ventaId, p_activo_oid);
                else
                    await new ActivoDigitalService(unitOfWork).BloquearParaVentaAsync(p_activo_oid);
            }

            TempData["Exito"] = ventaReutilizada is null
                ? $"«{nombreActivo}» está a la venta por {p_precio.Valor:N2} {p_precio.Divisa} (venta #{ventaId}). Has ganado 10 puntos de experiencia."
                : $"«{nombreActivo}» vuelve a estar a la venta por {p_precio.Valor:N2} {p_precio.Divisa} (venta #{ventaId}). La experiencia por publicar solo se gana la primera vez.";
            return RedirectToAction(nameof(PonerALaVenta));
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            await DeshacerVentaAsync(ventaCreada, ventaReutilizada, p_activo_oid);
            TempData["Error"] = this.MensajeDeError(error);
            return RedirectToAction(nameof(PonerALaVenta), new { activoId = p_activo_oid });
        }
    }

    /// <summary>Motivo por el que el usuario no puede poner el activo a la venta ahora, o null si puede.</summary>
    private static string? MotivoNoVendible(ActivoDigital activo, int usuarioId)
    {
        if (activo.Inventario?.Usuario?.Id != usuarioId) return "Solo puedes vender activos de tu inventario.";
        if (activo.EnVenta)
            return activo.Venta?.EstadoVenta == EstadoVenta.Disponible
                ? "Ya está a la venta."
                : "Está bloqueado: desbloquéalo desde sus operaciones para poder venderlo.";
        if (activo.IntercambioOfrecido is not null || activo.IntercambioSolicitado is not null)
            return "Está en un intercambio pendiente.";
        if (activo.Venta is not null
            && !(activo.Venta.EstadoVenta == EstadoVenta.Cancelada && activo.Venta.Vendedor?.Id == usuarioId))
            return "Ya se vendió una vez en SkinLink: el modelo solo admite una venta por activo.";
        return null;
    }

    private async Task<PonerALaVentaViewModel> CargarPonerALaVentaAsync(int usuarioId, int? activoId)
    {
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await unitOfWork.UsuarioRepository.ReadAllAsync();
        await unitOfWork.InventarioPrivadoRepository.ReadAllAsync();
        var ventas = await unitOfWork.VentaRepository.ReadAllAsync();
        var activos = await unitOfWork.ActivoDigitalRepository.ReadAllAsync();

        var model = new PonerALaVentaViewModel
        {
            Activos = activos
                .Where(a => a.Inventario?.Usuario?.Id == usuarioId)
                .OrderBy(a => a.Nombre, StringComparer.CurrentCultureIgnoreCase)
                .Select(a => new ActivoParaVenderItem
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    Detalle = $"{a.Coleccion} · {TextosDominio.Rareza(a.Atributos.Rareza)} · {TextosDominio.Condicion(a.Atributos.Condicion)}",
                    PrecioEstimado = a.PrecioEstimado?.Valor ?? 0,
                    Divisa = string.IsNullOrWhiteSpace(a.PrecioEstimado?.Divisa) ? "EUR" : a.PrecioEstimado.Divisa,
                    Motivo = MotivoNoVendible(a, usuarioId),
                })
                .ToList(),
            VentasPublicadas = ventas
                .Where(v => v.Vendedor?.Id == usuarioId && v.EstadoVenta == EstadoVenta.Disponible && v.ActivoDigital is not null && v.ActivoDigital.EnVenta)
                .OrderByDescending(v => v.FechaPublicacion)
                .Select(v => new VentaPublicadaItem
                {
                    VentaId = v.Id,
                    Activo = v.ActivoDigital.Nombre,
                    Precio = v.PrecioVenta?.Valor ?? 0,
                    Divisa = v.PrecioVenta?.Divisa ?? "EUR",
                    FechaPublicacion = v.FechaPublicacion,
                })
                .ToList(),
        };
        model.ActivoId = activoId is int pedido && model.Activos.Any(a => a.Id == pedido && a.Motivo is null)
            ? pedido
            : model.Activos.FirstOrDefault(a => a.Motivo is null)?.Id;
        return model;
    }

    /// <summary>Si la publicación falla, se deja todo como estaba (venta borrada o de nuevo cancelada).</summary>
    private async Task DeshacerVentaAsync(int? ventaCreada, int? ventaReutilizada, int activoId)
    {
        if (ventaCreada is null && ventaReutilizada is null) return;
        try
        {
            await using var unitOfWork = _formUnitOfWorkFactory.Create();
            var servicio = new VentaService(unitOfWork);
            if (ventaCreada is int creada)
            {
                await servicio.DestroyAsync(creada);
            }
            else if (ventaReutilizada is int reutilizada && await servicio.ReadOIDAsync(reutilizada) is Venta venta)
            {
                await servicio.ModifyAsync(reutilizada, venta.PrecioVenta, venta.FechaPublicacion, EstadoVenta.Cancelada, activoId);
            }
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.LogWarning(error, "No se ha podido deshacer la venta preparada para el activo {ActivoId}.", activoId);
        }
    }
}
