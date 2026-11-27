using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

        /*PROTECTED REGION ID(usingejemplo.Application.Services_Usuario_login) ENABLED START*/
        // references to other libraries
        /*PROTECTED REGION END*/

namespace ejemplo.Application.Services;

public sealed partial class UsuarioService
{
    public async Task<string?> LoginAsync(string p_correo, string p_password)
    {
        /*PROTECTED REGION ID(ejemplo.Application.Services_Usuario_login) ENABLED START*/
        if (string.IsNullOrWhiteSpace(p_correo) || string.IsNullOrEmpty(p_password))
            return null;

        try
        {
            var coincidencias = (await _repository.ReadAllAsync())
                .Where(usuario => usuario.Correo == p_correo)
                .Take(2)
                .ToList();
            if (coincidencias.Count != 1)
                return null; // inexistente o ambiguo: no se revela cuál de los dos

            var entity = coincidencias[0];
            if (!_passwordHasher.VerifyPassword(entity.Contrasena, p_password))
                return null;

            var tokenSubject = Convert.ToString(entity.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            var tokenClaims = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["id"] = tokenSubject,
                ["correo"] = entity.Correo,
            };
            return _tokenGenerator.CreateToken(tokenSubject, tokenClaims);
        }
        catch (Exception)
        {
            return null;
        }
        /*PROTECTED REGION END*/
    }
}
