using System.Globalization;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Skinlink.Web.Assemblers;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class UsuarioController
{
    [HttpGet]
    public IActionResult Perfil()
    {
        var id = User.GetUsuarioId();
        if (id is null) return Challenge();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> SumarExperiencia(int id)
    {
        var entity = await _usuarioService.ReadOIDAsync(id);
        if (entity is null) return NotFound();
        return View(new SumarExperienciaViewModel
        {
            UsuarioId = entity.Id,
            NombreUsuario = entity.NombreUsuario,
            NivelActual = entity.Nivel,
            ExperienciaActual = entity.Experiencia
        });
    }

    [HttpGet]
    public async Task<IActionResult> ReclamarRecompensaDiaria()
    {
        var id = User.GetUsuarioId();
        if (id is null) return Challenge();

        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        var usuario = await unitOfWork.UsuarioRepository.ReadAsync(id.Value);
        if (usuario is null) return NotFound();

        // Solo se ofrecen las cajas activas y gratuitas, que son las que admite la recompensa diaria.
        var cajas = (await unitOfWork.CajaMisteriosaRepository.ReadAllAsync())
            .Where(caja => caja.Activa && (caja.PrecioApertura is null || caja.PrecioApertura.Valor <= 0))
            .OrderBy(caja => caja.Nombre)
            .Select(caja => new SelectListItem(caja.Nombre, caja.Id.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        return View(new ReclamarRecompensaViewModel
        {
            UsuarioId = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            YaReclamadoHoy = YaReclamoHoy(usuario),
            Cajas = cajas,
            CajaId = cajas.Count == 1 ? int.Parse(cajas[0].Value, CultureInfo.InvariantCulture) : null
        });
    }

    private UsuarioPerfilViewModel ConstruirPerfil(Usuario entity) => new()
    {
        Usuario = UsuarioAssembler.ToViewModel(entity),
        // Herencia: un Administrador también es un Usuario y se muestra con sus datos propios.
        EsAdministrador = entity is Administrador,
        CodigoAuditor = (entity as Administrador)?.CodigoAuditor,
        SaldoDisponible = entity.Monedero?.SaldoDisponible?.Valor ?? 0,
        SaldoPendiente = entity.Monedero?.SaldoPendiente?.Valor ?? 0,
        Divisa = entity.Monedero?.SaldoDisponible?.Divisa ?? string.Empty,
        InventarioPublico = entity.Inventario?.EsPublico ?? false,
        EsMiPerfil = User.GetUsuarioId() == entity.Id,
        YaReclamadoHoy = YaReclamoHoy(entity)
    };

    // Misma regla que UsuarioUseCase.ReclamarRecompensaDiariaAsync (fecha UTC).
    private static bool YaReclamoHoy(Usuario usuario) =>
        usuario.FechaUltimaRecompensa.HasValue && usuario.FechaUltimaRecompensa.Value.Date == DateTime.UtcNow.Date;

    // Campos que en Alta/Registro no vienen del formulario: los rellena el servidor.
    private void QuitarCamposGestionadosPorServidor()
    {
        ModelState.Remove(nameof(UsuarioViewModel.FechaUltimaRecompensa));
        ModelState.Remove(nameof(UsuarioViewModel.MonederoId));
        ModelState.Remove(nameof(UsuarioViewModel.InventarioId));
    }

    private void QuitarCamposDelRegistro()
    {
        QuitarCamposGestionadosPorServidor();
        ModelState.Remove(nameof(UsuarioViewModel.Estado));
        ModelState.Remove(nameof(UsuarioViewModel.Nivel));
        ModelState.Remove(nameof(UsuarioViewModel.Experiencia));
        ModelState.Remove(nameof(UsuarioViewModel.Verificado));
        ModelState.Remove(nameof(UsuarioViewModel.FechaUltimoLogin));
    }

    private async Task<bool> CorreoEnUsoAsync(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo)) return false;
        var usuarios = await _usuarioService.ReadAllAsync();
        return usuarios.Any(usuario => string.Equals(usuario.Correo?.Trim(), correo.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}

public static class CuentaNueva
{
    public const string Divisa = "EUR";

    public static Monedero Monedero() => new()
    {
        SaldoDisponible = new Moneda { Valor = 0, Divisa = Divisa },
        SaldoPendiente = new Moneda { Valor = 0, Divisa = Divisa }
    };

    public static InventarioPrivado Inventario() => new()
    {
        ValorEstimadoTotal = new Moneda { Valor = 0, Divisa = Divisa },
        EsPublico = false
    };
}
