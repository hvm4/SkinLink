using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Usuario_sumarExperiencia) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class UsuarioService
{
    public async Task SumarExperienciaAsync(int p_oid, int p_puntos)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Usuario_sumarExperiencia) ENABLED START*/
        // Suma puntos de experiencia y recalcula el nivel (RF-04).
        // Regla única de nivel para toda la aplicación: nivel = experiencia / 100 + 1 (nunca baja).
        // No abre transacción propia: los casos de uso (compra, caja, recompensa, publicación) la llaman
        // dentro de la suya.
        if (p_puntos <= 0)
            throw new ModelException("Los puntos de experiencia a sumar deben ser positivos.");

        var usuario = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");

        if (usuario.Estado == EstadoCuenta.Suspendido)
            throw new ModelException("Una cuenta suspendida no puede ganar experiencia.");

        checked
        {
            usuario.Experiencia += p_puntos;
        }

        int nivelCalculado = usuario.Experiencia / 100 + 1;
        if (nivelCalculado > usuario.Nivel)
            usuario.Nivel = nivelCalculado;

        await _repository.SaveAsync(usuario);
        /*PROTECTED REGION END*/
    }
}
