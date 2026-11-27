using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Reglas;

public static class ReglasResenaVendedor
{
    public const int PuntuacionMinima = 1;
    public const int PuntuacionMaxima = 5;

    public static async Task ValidarNuevaResenaAsync(IUnitOfWork unitOfWork, int compradorId, int vendedorId, int puntuacion)
    {
        if (puntuacion < PuntuacionMinima || puntuacion > PuntuacionMaxima)
            throw new ModelException($"La puntuación debe estar entre {PuntuacionMinima} y {PuntuacionMaxima}.");

        if (compradorId == vendedorId)
            throw new ModelException("No puedes valorarte a ti mismo.");

        var comprador = await unitOfWork.UsuarioRepository.ReadAsync(compradorId)
            ?? throw new ModelException("El comprador no existe.");
        if (comprador.Estado != EstadoCuenta.Activo)
            throw new ModelException("Una cuenta suspendida no puede dejar reseñas.");

        _ = await unitOfWork.UsuarioRepository.ReadAsync(vendedorId)
            ?? throw new ModelException("El vendedor no existe.");

        var comprasCompletadas = (await unitOfWork.VentaRepository.ReadAllAsync())
            .Count(v => v.EstadoVenta == EstadoVenta.Completada
                        && v.Comprador?.Id == compradorId
                        && v.Vendedor?.Id == vendedorId);
        if (comprasCompletadas == 0)
            throw new ModelException("Solo puedes valorar a un vendedor al que le hayas comprado algo.");

        var resenasPrevias = (await unitOfWork.ResenaVendedorRepository.ReadAllAsync())
            .Count(r => r.Comprador?.Id == compradorId && r.Vendedor?.Id == vendedorId);
        if (resenasPrevias >= comprasCompletadas)
            throw new ModelException("Ya has valorado todas tus compras a este vendedor (una reseña por compra).");
    }
}
