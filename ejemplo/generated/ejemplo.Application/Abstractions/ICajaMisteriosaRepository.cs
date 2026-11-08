using ejemplo.Domain.Entities;

namespace ejemplo.Application.Abstractions;

public interface ICajaMisteriosaRepository : IRepository<CajaMisteriosa>
{
    Task<IReadOnlyList<CajaMisteriosa>> FiltrarCajasActivasAsync();
    Task<IReadOnlyList<CajaMisteriosa>> FiltrarCajasPorPrecioAperturaAsync(double p_precioMinimo, double p_precioMaximo);
}
