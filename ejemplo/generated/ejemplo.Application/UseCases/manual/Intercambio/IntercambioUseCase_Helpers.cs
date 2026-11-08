using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;

namespace ejemplo.Application.UseCases.Intercambio;

// Métodos auxiliares compartidos por IniciarIntercambio y AceptarIntercambio.
// Estaban en IntercambioUseCase_IniciarIntercambio.cs fuera de una región protegida,
// y HARIA los movió a .haria/orphans al regenerar. En este archivo propio no se pierden.
public sealed partial class IntercambioUseCase
{
    internal static List<ActivoDigital> ResolverActivos(IReadOnlyList<ActivoDigital> todos, IEnumerable<int> ids)
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
