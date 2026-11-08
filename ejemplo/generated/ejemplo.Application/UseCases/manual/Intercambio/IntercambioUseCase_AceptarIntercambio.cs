using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_Intercambio_aceptarIntercambio) ENABLED START*/
        using System.Linq;
        using System.Collections.Generic;
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.Intercambio;

public sealed partial class IntercambioUseCase
{
    public async Task AceptarIntercambioAsync(int p_oid, int p_receptor_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_Intercambio_aceptarIntercambio) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        IntercambioService intercambioService = null!;

        try
        {
            await _unitOfWork.BeginTransactionAsync("IntercambioUseCase.AceptarIntercambioAsync");
            intercambioService = new IntercambioService(_unitOfWork);

            var intercambio = await _unitOfWork.IntercambioRepository.ReadAsync(p_oid)
                ?? throw new ModelException("Intercambio not found.");

            await _unitOfWork.UsuarioRepository.ReadAllAsync();
            var inventarios = await _unitOfWork.InventarioPrivadoRepository.ReadAllAsync();
            var activos = await _unitOfWork.ActivoDigitalRepository.ReadAllAsync();

            if (intercambio.Estado != EstadoIntercambio.Pendiente)
                throw new ModelException("El intercambio ya no esta pendiente.");

            if (intercambio.Receptor?.Id != p_receptor_oid)
                throw new ModelException("Solo el receptor del intercambio puede aceptarlo.");

            int proponenteId = intercambio.Proponente?.Id
                ?? throw new ModelException("El intercambio no tiene proponente.");

            if (intercambio.Receptor!.Estado != EstadoCuenta.Activo)
                throw new ModelException("La cuenta del receptor no está activa.");
            if (intercambio.Proponente!.Estado != EstadoCuenta.Activo)
                throw new ModelException("La cuenta del proponente no está activa.");

            List<ActivoDigital> ofrecidos = activos.Where(a => a.IntercambioOfrecido?.Id == intercambio.Id).ToList();
            List<ActivoDigital> solicitados = activos.Where(a => a.IntercambioSolicitado?.Id == intercambio.Id).ToList();

            if (ofrecidos.Count == 0 || solicitados.Count == 0)
                throw new ModelException("El intercambio no tiene activos en ambos lados.");

            InventarioPrivado invProponente = ValidarPropiedad(inventarios, proponenteId, ofrecidos);
            InventarioPrivado invReceptor = ValidarPropiedad(inventarios, p_receptor_oid, solicitados);

            foreach (ActivoDigital a in ofrecidos.Concat(solicitados))
            {
                if (a.EnVenta)
                    throw new ModelException($"El activo {a.Id} esta publicado en venta y no puede intercambiarse.");
            }

            foreach (ActivoDigital a in ofrecidos)
            {
                a.Inventario = invReceptor;
                a.IntercambioOfrecido = null;
                await _unitOfWork.ActivoDigitalRepository.SaveAsync(a);
            }

            foreach (ActivoDigital a in solicitados)
            {
                a.Inventario = invProponente;
                a.IntercambioSolicitado = null;
                await _unitOfWork.ActivoDigitalRepository.SaveAsync(a);
            }

            intercambio.Estado = EstadoIntercambio.Aceptado;
            await _unitOfWork.IntercambioRepository.SaveAsync(intercambio);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Intercambio.aceptarIntercambio.", ex);
        }
        /*PROTECTED REGION END*/
    }
}
