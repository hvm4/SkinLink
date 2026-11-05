using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ejemplo.Application.Abstractions;
using ejemplo.Infrastructure.Data;

namespace ejemplo.Infrastructure;

public sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly DbContextOptions<ejemploDbContext> _options;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly IHariaPasswordHasher? _passwordHasher;
    private readonly IHariaTokenGenerator? _tokenGenerator;

    public UnitOfWorkFactory(DbContextOptions<ejemploDbContext> options, ILoggerFactory? loggerFactory = null, IHariaPasswordHasher? passwordHasher = null, IHariaTokenGenerator? tokenGenerator = null)
    {
        _options = options;
        _loggerFactory = loggerFactory;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    internal ILoggerFactory? LoggerFactory => _loggerFactory;

    internal IHariaPasswordHasher? PasswordHasher => _passwordHasher;

    internal IHariaTokenGenerator? TokenGenerator => _tokenGenerator;

    internal ejemploDbContext CreateDbContext() =>
        new(_options);

    public IUnitOfWork Create() =>
        new UnitOfWork(this);
}