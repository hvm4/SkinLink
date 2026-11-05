using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Abstractions;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<IReadOnlyList<Usuario>> FiltrarUsuariosPorEstadoAsync(EstadoCuenta p_estado);
}
