// Fichero manual (no lo genera HARIA Web): vistas de los filtros y operaciones de CajaMisteriosa.
// Las acciones generadas (FiltrarCajas..., SimularApertura, AbrirCajaSorpresa) delegan en estos
// métodos desde sus regiones protegidas.
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
    /// <summary>Listado de cajas con los filtros (activas / por precio). Visible sin iniciar sesión.</summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Buscar() => View("Buscar", new CajaBusquedaViewModel());

    /// <summary>Página de una caja: probabilidades legibles (RF-20), simulación (RF-22) y apertura (RF-19).</summary>
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

    private IActionResult MostrarBusquedaCajas(CajaBusquedaViewModel model, string filtro, IEnumerable<CajaMisteriosa> resultados)
    {
        model.FiltroAplicado = filtro;
        model.Resultados = resultados
            .OrderBy(c => c.PrecioApertura?.Valor ?? 0)
            .Select(CajaMisteriosaAssembler.ToViewModel)
            .ToList();
        return View("Buscar", model);
    }

    /// <summary>
    /// Lee un número de la URL aceptando coma o punto decimal (los formularios GET envían el texto tal
    /// cual y ASP.NET interpreta la query string con cultura invariante).
    /// </summary>
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
    /// <summary>
    /// CU-19 / RF-25: al crear o editar una caja, la suma de los porcentajes de sus probabilidades
    /// debe ser exactamente 100 % (con tolerancia de redondeo) y ninguno puede ser negativo o mayor que 100.
    /// </summary>
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
