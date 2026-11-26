// Fichero manual (no lo genera HARIA Web) — Miembro 4: calculadora de la comisión de mercado.
// Usa calcularComisionMercado (porcentaje según el tramo de precio de la configuración del sistema).
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using ejemplo.Domain.Entities;
using Skinlink.Web.ViewModels;

namespace Skinlink.Web.Controllers;

public partial class ConfiguracionSistemaController
{
    [HttpGet]
    public async Task<IActionResult> Comision()
    {
        var configuracion = (await _configuracionSistemaService.ReadAllAsync()).FirstOrDefault();
        if (configuracion is null)
        {
            TempData["Error"] = "Todavía no hay configuración del sistema: créala antes de calcular comisiones.";
            return RedirectToAction(nameof(Index));
        }
        return View("Comision", AVista(configuracion));
    }

    private async Task<IActionResult> CalcularComisionEnVistaAsync(int configuracionId)
    {
        var configuracion = await _configuracionSistemaService.ReadOIDAsync(configuracionId);
        if (configuracion is null) return NotFound();

        var model = AVista(configuracion);
        model.Precio = LeerDecimal(Request.Form["p_precio"].ToString());

        // El precio se lee a mano: el binding en es-ES tomaría "96.40" como 9640.
        ModelState.Clear();
        if (model.Precio is not double precio || precio <= 0)
        {
            ModelState.AddModelError(nameof(model.Precio), "Introduce un precio mayor que 0 (por ejemplo 96,40).");
            return View("Comision", model);
        }

        try
        {
            var porcentaje = await _configuracionSistemaService.CalcularComisionMercadoAsync(configuracionId, precio);
            // Mismo redondeo que procesarCompraSkin al cobrar la comisión.
            var comision = Math.Round(precio * porcentaje / 100d, 2);
            model.Porcentaje = porcentaje;
            model.ImporteComision = comision;
            model.ImporteNeto = precio - comision;
        }
        catch (Exception ex) when (MensajesError.EsDeNegocio(ex))
        {
            ModelState.AddModelError(nameof(model.Precio), MensajesError.Mensaje(ex));
        }

        return View("Comision", model);
    }

    private static double? LeerDecimal(string texto)
    {
        texto = texto.Trim().Replace("€", string.Empty).Trim();
        if (texto.Length == 0) return null;
        return double.TryParse(texto.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : null;
    }

    private static ComisionMercadoViewModel AVista(ConfiguracionSistema c) => new()
    {
        ConfiguracionId = c.Id,
        LimitePrecioBajo = c.LimitePrecioBajo,
        LimitePrecioMedio = c.LimitePrecioMedio,
        ComisionPrecioBajo = c.ComisionPrecioBajo,
        ComisionPrecioMedio = c.ComisionPrecioMedio,
        ComisionPrecioAlto = c.ComisionPrecioAlto
    };
}
