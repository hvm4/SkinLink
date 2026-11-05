using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_InventarioPrivado_cambiarVisibilidadInventario) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class InventarioPrivadoService
{
    public async Task<bool> CambiarVisibilidadInventarioAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_InventarioPrivado_cambiarVisibilidadInventario) ENABLED START*/
        InventarioPrivado inventario = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("InventarioPrivado not found.");

        inventario.EsPublico = !inventario.EsPublico;
        await _repository.SaveAsync(inventario);
        return inventario.EsPublico;

        throw new ModelException("Application service generation for InventarioPrivado.cambiarVisibilidadInventario is pending.", new NotImplementedException("Method CambiarVisibilidadInventarioAsync() not yet implemented."));
        /*PROTECTED REGION END*/
    }
}
