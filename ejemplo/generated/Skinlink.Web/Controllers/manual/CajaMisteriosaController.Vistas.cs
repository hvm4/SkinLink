using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ejemplo.Domain.Entities;
using Skinlink.Web.Assemblers;
using Skinlink.Web.Utils.Security;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class CajaMisteriosaController
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Buscar() => View("Buscar", new CajaBusquedaViewModel());

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Abrir(int id)
    {
        ActivoDigital? obtenido = null;
        if (TempData["CajaActivoObtenidoId"] is int obtenidoId)
        {
            await using var uow = _formUnitOfWorkFactory.Create();
            obtenido = await uow.ActivoDigitalRepository.ReadAsync(obtenidoId);
        }
        return await MostrarAperturaAsync(id, obtenido, esSimulacion: false);
    }

    private int? UsuarioActualId() => User.GetUsuarioId();

    private async Task<IActionResult> MostrarBusquedaCajasAsync(CajaBusquedaViewModel model, string filtro, IEnumerable<CajaMisteriosa> resultados)
    {
        model.FiltroAplicado = filtro;
        model.Resultados = resultados
            .OrderBy(c => c.PrecioApertura?.Valor ?? 0)
            .Select(CajaMisteriosaAssembler.ToViewModel)
            .ToList();

        // Los filtros no cargan las probabilidades de cada caja: se cuentan aparte.
        await using var unitOfWork = _formUnitOfWorkFactory.Create();
        await unitOfWork.CajaMisteriosaRepository.ReadAllAsync();
        var probabilidades = await unitOfWork.ProbabilidadDropRepository.ReadAllAsync();
        model.Recompensas = probabilidades
            .Where(p => p.Caja is not null)
            .GroupBy(p => p.Caja.Id)
            .ToDictionary(g => g.Key, g => g.Count());
        return View("Buscar", model);
    }

    private double? LeerDecimalDeQuery(string clave)
    {
        var texto = Request.Query[clave].ToString().Trim();
        if (texto.Length == 0) return null;
        return double.TryParse(texto.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : null;
    }

    private async Task<IActionResult> MostrarAperturaAsync(int id, ActivoDigital? resultado, bool esSimulacion)
    {
        await using var uow = _formUnitOfWorkFactory.Create();
        var caja = await uow.CajaMisteriosaRepository.ReadAsync(id); // carga probabilidades y activos
        if (caja is null) return NotFound();

        var model = new CajaAperturaViewModel
        {
            Caja = CajaMisteriosaAssembler.ToViewModel(caja),
            Probabilidades = caja.Probabilidades
                .Where(p => p.ActivoRecompensa is not null)
                .OrderByDescending(p => p.PorcentajeDrop)
                .Select(p => new ProbabilidadFilaViewModel
                {
                    ActivoId = p.ActivoRecompensa.Id,
                    Nombre = p.ActivoRecompensa.Nombre,
                    Coleccion = p.ActivoRecompensa.Coleccion,
                    Rareza = p.ActivoRecompensa.Atributos?.Rareza ?? default,
                    Porcentaje = p.PorcentajeDrop,
                    Imagen = p.ActivoRecompensa.Imagen
                })
                .ToList(),
            Autenticado = User.Identity?.IsAuthenticated == true,
            Resultado = resultado is null ? null : ActivoDigitalAssembler.ToViewModel(resultado),
            ResultadoEsSimulacion = esSimulacion
        };

        if (UsuarioActualId() is int usuarioId)
        {
            var usuario = await uow.UsuarioRepository.ReadAsync(usuarioId); // carga su monedero
            model.SaldoUsuario = usuario?.Monedero?.SaldoDisponible?.Valor;
            model.DivisaUsuario = usuario?.Monedero?.SaldoDisponible?.Divisa;
        }

        return View("Abrir", model);
    }
}

public partial class CajaMisteriosaController
{
    private void ValidarSumaProbabilidades(Skinlink.Web.ViewModels.CajaMisteriosaViewModel model)
    {
        var filas = model.Probabilidades ?? new();
        for (var i = 0; i < filas.Count; i++)
        {
            var porcentaje = filas[i].PorcentajeDrop;
            if (double.IsNaN(porcentaje) || porcentaje <= 0 || porcentaje > 100)
                ModelState.AddModelError($"Probabilidades[{i}].PorcentajeDrop", "Cada probabilidad debe estar entre 0 (excluido) y 100.");
        }

        var suma = filas.Sum(f => f.PorcentajeDrop);
        if (filas.Count > 0 && Math.Abs(suma - 100d) > 0.01)
            ModelState.AddModelError(nameof(model.Probabilidades),
                $"Las probabilidades deben sumar 100 % (ahora suman {suma:0.##} %).");
    }
}
