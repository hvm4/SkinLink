using ejemplo.Domain.Entities;

namespace ejemplo.Application.Abstractions;

public interface IResenaVendedorRepository : IRepository<ResenaVendedor>
{
    Task<IReadOnlyList<ResenaVendedor>> FiltrarResenasVisiblesPorVendedorAsync(int p_vendedorId);
}
