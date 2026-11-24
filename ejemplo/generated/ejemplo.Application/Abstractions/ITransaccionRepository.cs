using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Abstractions;

public interface ITransaccionRepository : IRepository<Transaccion>
{
    Task<IReadOnlyList<Transaccion>> FiltrarTransaccionesPorTipoAsync(int p_usuarioId, TipoTransaccion p_tipo);
    Task<IReadOnlyList<Transaccion>> FiltrarTransaccionesPorRangoFechasAsync(int p_usuarioId, DateTime p_fechaInicio, DateTime p_fechaFin);
}
