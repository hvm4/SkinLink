using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ejemplo.Application.Abstractions;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;
using ejemplo.Infrastructure.Repositories;
using ejemplo.Infrastructure.Security;

namespace ejemplo.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly ejemploDbContext _dbContext;
    private readonly ILogger<UnitOfWork>? _logger;
    private readonly ILoggerFactory? _loggerFactory;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(UnitOfWorkFactory unitOfWorkFactory)
        : this(
            unitOfWorkFactory.CreateDbContext(),
            unitOfWorkFactory.LoggerFactory,
            unitOfWorkFactory.PasswordHasher,
            unitOfWorkFactory.TokenGenerator)
    {
    }

    public UnitOfWork(ejemploDbContext dbContext, ILoggerFactory? loggerFactory = null, IHariaPasswordHasher? passwordHasher = null, IHariaTokenGenerator? tokenGenerator = null)
    {
        _dbContext = dbContext;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory?.CreateLogger<UnitOfWork>();
        PasswordHasher = passwordHasher ?? new HariaPasswordHasher();
        TokenGenerator = tokenGenerator ?? new HariaTokenGenerator();
        UsuarioRepository = new UsuarioRepository(dbContext, _loggerFactory?.CreateLogger<UsuarioRepository>());
        AdministradorRepository = new AdministradorRepository(dbContext, _loggerFactory?.CreateLogger<AdministradorRepository>());
        MonederoRepository = new MonederoRepository(dbContext, _loggerFactory?.CreateLogger<MonederoRepository>());
        TransaccionRepository = new TransaccionRepository(dbContext, _loggerFactory?.CreateLogger<TransaccionRepository>());
        InventarioPrivadoRepository = new InventarioPrivadoRepository(dbContext, _loggerFactory?.CreateLogger<InventarioPrivadoRepository>());
        ArticuloCatalogoRepository = new ArticuloCatalogoRepository(dbContext, _loggerFactory?.CreateLogger<ArticuloCatalogoRepository>());
        ActivoDigitalRepository = new ActivoDigitalRepository(dbContext, _loggerFactory?.CreateLogger<ActivoDigitalRepository>());
        FavoritoRepository = new FavoritoRepository(dbContext, _loggerFactory?.CreateLogger<FavoritoRepository>());
        IntercambioRepository = new IntercambioRepository(dbContext, _loggerFactory?.CreateLogger<IntercambioRepository>());
        VentaRepository = new VentaRepository(dbContext, _loggerFactory?.CreateLogger<VentaRepository>());
        ResenaVendedorRepository = new ResenaVendedorRepository(dbContext, _loggerFactory?.CreateLogger<ResenaVendedorRepository>());
        CajaMisteriosaRepository = new CajaMisteriosaRepository(dbContext, _loggerFactory?.CreateLogger<CajaMisteriosaRepository>());
        ProbabilidadDropRepository = new ProbabilidadDropRepository(dbContext, _loggerFactory?.CreateLogger<ProbabilidadDropRepository>());
        ConfiguracionSistemaRepository = new ConfiguracionSistemaRepository(dbContext, _loggerFactory?.CreateLogger<ConfiguracionSistemaRepository>());
        RegistroAuditoriaRepository = new RegistroAuditoriaRepository(dbContext, _loggerFactory?.CreateLogger<RegistroAuditoriaRepository>());
    }

    public IUsuarioRepository UsuarioRepository { get; }
    public IAdministradorRepository AdministradorRepository { get; }
    public IMonederoRepository MonederoRepository { get; }
    public ITransaccionRepository TransaccionRepository { get; }
    public IInventarioPrivadoRepository InventarioPrivadoRepository { get; }
    public IArticuloCatalogoRepository ArticuloCatalogoRepository { get; }
    public IActivoDigitalRepository ActivoDigitalRepository { get; }
    public IFavoritoRepository FavoritoRepository { get; }
    public IIntercambioRepository IntercambioRepository { get; }
    public IVentaRepository VentaRepository { get; }
    public IResenaVendedorRepository ResenaVendedorRepository { get; }
    public ICajaMisteriosaRepository CajaMisteriosaRepository { get; }
    public IProbabilidadDropRepository ProbabilidadDropRepository { get; }
    public IConfiguracionSistemaRepository ConfiguracionSistemaRepository { get; }
    public IRegistroAuditoriaRepository RegistroAuditoriaRepository { get; }

    public IHariaPasswordHasher PasswordHasher { get; }

    public IHariaTokenGenerator TokenGenerator { get; }

    public async Task BeginTransactionAsync(string? businessOperation = null)
    {
        try
        {
            _transaction ??= await _dbContext.Database.BeginTransactionAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error beginning Entity Framework transaction.");
            throw new DataException("Error beginning Entity Framework transaction.", ex);
        }
    }

    public async Task CommitAsync()
    {
        try
        {
            await _dbContext.SaveChangesAsync();
            if (_transaction is not null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error committing Entity Framework transaction.");
            throw new DataException("Error committing Entity Framework transaction.", ex);
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error rolling back Entity Framework transaction.");
            throw new DataException("Error rolling back Entity Framework transaction.", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_transaction is not null)
        {
            await RollbackAsync();
        }

        await _dbContext.DisposeAsync();
        _disposed = true;
    }
}
