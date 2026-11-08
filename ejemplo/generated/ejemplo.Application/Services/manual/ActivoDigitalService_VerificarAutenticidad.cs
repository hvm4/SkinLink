using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

/*PROTECTED REGION ID(usingejemplo.Application.Services_ActivoDigital_verificarAutenticidad) ENABLED START*/
// references to other libraries
/*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ActivoDigitalService
{
    public async Task<bool> VerificarAutenticidadAsync(int p_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ActivoDigital_verificarAutenticidad) ENABLED START*/
        ActivoDigital activo = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ActivoDigital not found.");

        if (string.IsNullOrWhiteSpace(activo.IdentificadorUnico) ||
            !Guid.TryParse(activo.IdentificadorUnico, out _))
        {
            return false;
        }

        var todos = await _repository.ReadAllAsync();
        int coincidencias = todos.Count(a =>
            string.Equals(a.IdentificadorUnico, activo.IdentificadorUnico, StringComparison.OrdinalIgnoreCase));
        return coincidencias == 1;
        /*PROTECTED REGION END*/
    }
}