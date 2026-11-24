// Fichero manual (no lo genera HARIA Web): ayudas comunes de los controladores de operaciones de
// monedero y venta. Está fuera de Controllers/manual para que la regeneración no lo toque.
using System.Globalization;
using System.Security.Claims;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Utils;

public enum ModoVentas
{
    /// <summary>Ventas que el usuario puede gestionar (las suyas; el administrador, todas).</summary>
    Gestionar,
    /// <summary>Ventas que el usuario puede comprar (las de los demás).</summary>
    Comprar
}

public static class CompraventaWebHelper
{
    /// <summary>Los casos de uso envuelven el error real en InnerException: se devuelve el más interno.</summary>
    public static string MensajeDeError(Exception ex)
    {
        var actual = ex;
        while (actual.InnerException is not null && !string.IsNullOrWhiteSpace(actual.InnerException.Message))
            actual = actual.InnerException;
        return actual.Message;
    }

    public static async Task<OperacionMonederoViewModel> CargarMonederosAsync(
        IUnitOfWorkFactory factory, ClaimsPrincipal user, int? seleccionado)
    {
        await using var unitOfWork = factory.Create();
        await unitOfWork.UsuarioRepository.ReadAllAsync();
        var monederos = await unitOfWork.MonederoRepository.ReadAllAsync();

        var vm = new OperacionMonederoViewModel();
        IEnumerable<Monedero> lista = monederos;
        if (!user.EsAdministrador())
        {
            var usuarioId = user.GetUsuarioId();
            lista = monederos.Where(m => usuarioId is not null && m.Usuario?.Id == usuarioId);
            vm.MonederoFijo = true;
        }

        var items = lista.OrderBy(m => m.Id).ToList();
        vm.MonederoId = seleccionado is not null && items.Any(m => m.Id == seleccionado)
            ? seleccionado
            : (vm.MonederoFijo ? items.FirstOrDefault()?.Id : null);

        vm.Monederos = items.Select(m => new SelectListItem
        {
            Value = m.Id.ToString(CultureInfo.InvariantCulture),
            Text = $"#{m.Id} · {m.Usuario?.NombreUsuario} · saldo {m.SaldoDisponible?.Valor.ToString("N2")} {m.SaldoDisponible?.Divisa}",
            Selected = m.Id == vm.MonederoId
        }).ToList();

        vm.Divisa = items.FirstOrDefault(m => m.Id == vm.MonederoId)?.SaldoDisponible?.Divisa
                    ?? items.FirstOrDefault()?.SaldoDisponible?.Divisa
                    ?? "EUR";
        return vm;
    }

    public static async Task<bool> PuedeUsarMonederoAsync(
        IUnitOfWorkFactory factory, ClaimsPrincipal user, int monederoId)
    {
        if (user.EsAdministrador()) return true;
        var usuarioId = user.GetUsuarioId();
        if (usuarioId is null) return false;

        await using var unitOfWork = factory.Create();
        await unitOfWork.UsuarioRepository.ReadAllAsync();
        var monedero = await unitOfWork.MonederoRepository.ReadAsync(monederoId);
        return monedero?.Usuario?.Id == usuarioId;
    }

    public static async Task<OperacionVentaViewModel> CargarVentasAsync(
        IUnitOfWorkFactory factory, ClaimsPrincipal user, ModoVentas modo,
        Func<Venta, bool> predicado, int? seleccionada)
    {
        await using var unitOfWork = factory.Create();
        await unitOfWork.UsuarioRepository.ReadAllAsync();
        await unitOfWork.ActivoDigitalRepository.ReadAllAsync();
        var ventas = await unitOfWork.VentaRepository.ReadAllAsync();

        var esAdmin = user.EsAdministrador();
        var usuarioId = user.GetUsuarioId();
        var vm = new OperacionVentaViewModel { EsAdministrador = esAdmin };

        IEnumerable<Venta> lista = ventas.Where(predicado);
        if (!esAdmin)
        {
            lista = modo == ModoVentas.Gestionar
                ? lista.Where(v => usuarioId is not null && v.Vendedor?.Id == usuarioId)
                : lista.Where(v => v.Vendedor?.Id != usuarioId);
        }

        vm.Ventas = lista.OrderBy(v => v.Id).Select(v => new VentaOperacionItem
        {
            VentaId = v.Id,
            ActivoId = v.ActivoDigital?.Id ?? 0,
            Texto = $"#{v.Id} · {v.ActivoDigital?.Nombre} · vendedor {v.Vendedor?.NombreUsuario} · {v.PrecioVenta?.Valor.ToString("N2")} {v.PrecioVenta?.Divisa} · {v.EstadoVenta}",
            Precio = v.PrecioVenta?.Valor ?? 0,
            Divisa = v.PrecioVenta?.Divisa ?? "EUR"
        }).ToList();

        vm.VentaId = seleccionada is not null && vm.Ventas.Any(v => v.VentaId == seleccionada) ? seleccionada : null;

        if (modo == ModoVentas.Comprar)
        {
            var usuarios = await unitOfWork.UsuarioRepository.ReadAllAsync();
            if (esAdmin)
            {
                vm.Compradores = usuarios.OrderBy(u => u.Id)
                    .Select(u => new SelectListItem(u.NombreUsuario, u.Id.ToString(CultureInfo.InvariantCulture)))
                    .ToList();
            }
            else if (usuarioId is not null)
            {
                vm.CompradorId = usuarioId;
                vm.CompradorNombre = usuarios.FirstOrDefault(u => u.Id == usuarioId)?.NombreUsuario ?? string.Empty;
            }
        }
        return vm;
    }

    public static async Task<bool> PuedeGestionarVentaAsync(
        IUnitOfWorkFactory factory, ClaimsPrincipal user, int ventaId)
    {
        if (user.EsAdministrador()) return true;
        var usuarioId = user.GetUsuarioId();
        if (usuarioId is null) return false;

        await using var unitOfWork = factory.Create();
        await unitOfWork.UsuarioRepository.ReadAllAsync();
        var venta = await unitOfWork.VentaRepository.ReadAsync(ventaId);
        return venta?.Vendedor?.Id == usuarioId;
    }
}
