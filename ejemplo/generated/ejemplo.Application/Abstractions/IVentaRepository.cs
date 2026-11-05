using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Abstractions;

public interface IVentaRepository : IRepository<Venta>
{
    Task<IReadOnlyList<Venta>> FiltrarVentasPorEstadoAsync(EstadoVenta p_estado);
    Task<IReadOnlyList<Venta>> FiltrarVentasPorPrecioMaximoAsync(double p_precioMaximo);
}
