using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ResenaVendedor_ocultarResena) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ResenaVendedorService
{
    public async Task OcultarResenaAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ResenaVendedor_ocultarResena) ENABLED START*/
        // Oculta una reseña del perfil público del vendedor (RF-26). No se borra: se conserva para auditoría.
        // Para restaurarla o dejar constancia del administrador, usar moderarResena.
        var resena = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ResenaVendedor not found.");

        if (resena.Oculta)
            throw new ModelException("La reseña ya está oculta.");

        resena.Oculta = true;
        await _repository.SaveAsync(resena);
        /*PROTECTED REGION END*/
    }
}
