using ejemplo.Application.Abstractions;
using ejemplo.Infrastructure.Utils;

namespace ejemplo.Infrastructure.Security;

public sealed class HariaPasswordHasher : IHariaPasswordHasher
{
    public string HashPasswordIfNeeded(string password) =>
        HariaPasswordHashingUtils.HashPasswordIfNeeded(password);

    public string HashPassword(string password) =>
        HariaPasswordHashingUtils.HashPassword(password);

    public bool VerifyPassword(string? storedHash, string? password) =>
        HariaPasswordHashingUtils.VerifyPassword(storedHash, password);
}