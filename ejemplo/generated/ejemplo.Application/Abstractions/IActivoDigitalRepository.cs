using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Abstractions;

public interface IActivoDigitalRepository : IRepository<ActivoDigital>
{
    Task<IReadOnlyList<ActivoDigital>> FiltrarActivosPorRarezaAsync(Rareza p_rareza);
    Task<IReadOnlyList<ActivoDigital>> FiltrarActivosPorColeccionAsync(string p_coleccion);
    Task<IReadOnlyList<ActivoDigital>> FiltrarActivosPorCondicionAsync(Condicion p_condicion);
}
