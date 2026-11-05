using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Abstractions;

public interface ITransaccionRepository : IRepository<Transaccion>
{
    Task<IReadOnlyList<Transaccion>> FiltrarTransaccionesPorTipoAsync(TipoTransaccion p_tipo);
    Task<IReadOnlyList<Transaccion>> FiltrarTransaccionesPorRangoFechasAsync(DateTime p_fechaInicio, DateTime p_fechaFin);
}
