using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Application.Services;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.UseCases_Intercambio_iniciarIntercambio) ENABLED START*/
        using System.Linq;
        using System.Collections.Generic;
        /*PROTECTED REGION END*/

namespace ejemplo.Application.UseCases.Intercambio;

public sealed partial class IntercambioUseCase
{
    public async Task<global::ejemplo.Domain.Entities.Intercambio> IniciarIntercambioAsync(int p_proponente_oid, int p_receptor_oid, IReadOnlyList<int> p_activosOfrecidos_oids, IReadOnlyList<int> p_activosSolicitados_oids)
    {
        /*PROTECTED REGION ID(ejemplo.Application.UseCases_Intercambio_iniciarIntercambio) ENABLED START*/
        // Resolve external infrastructure services with _utils!.GetRequiredService<TService>() when this use case receives IUnitOfWorkUtils.
        IntercambioService intercambioService = null!;
        global::ejemplo.Domain.Entities.Intercambio result = default(global::ejemplo.Domain.Entities.Intercambio)!;

        if (p_proponente_oid == p_receptor_oid)
            throw new ModelException("El proponente y el receptor no pueden ser el mismo usuario.");

        List<int> ofrecidosIds = (p_activosOfrecidos_oids ?? new List<int>()).Distinct().ToList();
        List<int> solicitadosIds = (p_activosSolicitados_oids ?? new List<int>()).Distinct().ToList();

        if (ofrecidosIds.Count == 0 || solicitadosIds.Count == 0)
            throw new ModelException("Un intercambio necesita al menos un activo ofrecido y uno solicitado.");
        if (ofrecidosIds.Intersect(solicitadosIds).Any())
            throw new ModelException("Un activo no puede estar a la vez ofrecido y solicitado.");

        try
        {
            await _unitOfWork.BeginTransactionAsync("IntercambioUseCase.IniciarIntercambioAsync");
            intercambioService = new IntercambioService(_unitOfWork);

            Usuario proponente = await _unitOfWork.UsuarioRepository.ReadAsync(p_proponente_oid)
                ?? throw new ModelException("Usuario proponente not found.");
            Usuario receptor = await _unitOfWork.UsuarioRepository.ReadAsync(p_receptor_oid)
                ?? throw new ModelException("Usuario receptor not found.");

            var inventarios = await _unitOfWork.InventarioPrivadoRepository.ReadAllAsync();
            await _unitOfWork.IntercambioRepository.ReadAllAsync();
            var activos = await _unitOfWork.ActivoDigitalRepository.ReadAllAsync();

            List<ActivoDigital> ofrecidos = ResolverActivos(activos, ofrecidosIds);
            List<ActivoDigital> solicitados = ResolverActivos(activos, solicitadosIds);

            ValidarPropiedad(inventarios, p_proponente_oid, ofrecidos);
            ValidarPropiedad(inventarios, p_receptor_oid, solicitados);

            foreach (ActivoDigital a in ofrecidos.Concat(solicitados))
            {
                if (a.EnVenta)
                    throw new ModelException($"El activo {a.Id} esta publicado en venta y no puede intercambiarse.");
                if (a.IntercambioOfrecido is not null || a.IntercambioSolicitado is not null)
                    throw new ModelException($"El activo {a.Id} ya participa en otro intercambio pendiente.");
            }

            result = new global::ejemplo.Domain.Entities.Intercambio
            {
                Estado = EstadoIntercambio.Pendiente,
                FechaCreacion = DateTime.UtcNow,
                Proponente = proponente,
                Receptor = receptor,
            };
            await _unitOfWork.IntercambioRepository.SaveAsync(result);

            foreach (ActivoDigital a in ofrecidos)
            {
                a.IntercambioOfrecido = result;
                await _unitOfWork.ActivoDigitalRepository.SaveAsync(a);
            }
            foreach (ActivoDigital a in solicitados)
            {
                a.IntercambioSolicitado = result;
                await _unitOfWork.ActivoDigitalRepository.SaveAsync(a);
            }

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new ModelException("Application error while executing use case Intercambio.iniciarIntercambio.", ex);
        }

        return result;
        /*PROTECTED REGION END*/
    }

    private static List<ActivoDigital> ResolverActivos(IReadOnlyList<ActivoDigital> todos, IEnumerable<int> ids)
    {
        List<ActivoDigital> resultado = new();
        foreach (int id in ids)
        {
            ActivoDigital activo = todos.FirstOrDefault(a => a.Id == id)
                ?? throw new ModelException($"ActivoDigital {id} not found.");
            resultado.Add(activo);
        }
        return resultado;
    }

    internal static InventarioPrivado ValidarPropiedad(
        IReadOnlyList<InventarioPrivado> inventarios, int usuarioOid, IEnumerable<ActivoDigital> activos)
    {
        InventarioPrivado inventario = inventarios.FirstOrDefault(i => i.Usuario?.Id == usuarioOid)
            ?? throw new ModelException($"El usuario {usuarioOid} no tiene inventario.");

        foreach (ActivoDigital activo in activos)
        {
            if (activo.Inventario?.Id != inventario.Id)
                throw new ModelException($"El activo {activo.Id} no pertenece al usuario {usuarioOid}.");
        }
        return inventario;
    }
}