using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_ActivoDigital_marcarComoFavorito) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class ActivoDigitalService
{
    public async Task<bool> MarcarComoFavoritoAsync(int p_oid, int p_usuario_oid)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_ActivoDigital_marcarComoFavorito) ENABLED START*/
        ActivoDigital activo = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ActivoDigital not found.");
        Usuario usuario = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found.");

        var favoritos = await _unitOfWork.FavoritoRepository.ReadAllAsync();
        Favorito? existente = favoritos.FirstOrDefault(f =>
            f.Usuario?.Id == p_usuario_oid && f.ActivoDigital?.Id == p_oid);

        if (existente is not null)
        {
            await _unitOfWork.FavoritoRepository.DeleteAsync(existente);
            return false;
        }

        Favorito nuevo = new Favorito
        {
            FechaMarcado = DateTime.UtcNow,
            Usuario = usuario,
            ActivoDigital = activo,
        };
        await _unitOfWork.FavoritoRepository.SaveAsync(nuevo);
        return true;
        /*PROTECTED REGION END*/
    }
}
