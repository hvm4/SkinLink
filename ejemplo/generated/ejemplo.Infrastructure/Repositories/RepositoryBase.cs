using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity>
    where TEntity : class
{
    private readonly ejemploDbContext? _dbContext;

    protected RepositoryBase(ILogger? logger = null)
        : this(CreateDbContext(), logger)
    {
    }

    protected RepositoryBase(ejemploDbContext dbContext, ILogger? logger = null)
    {
        _dbContext = dbContext;
        Logger = logger;
    }

    protected ejemploDbContext DbContext =>
        _dbContext ?? throw new InvalidOperationException("No Entity Framework DbContext was provided.");

    protected ILogger? Logger { get; }

    protected string EntityName => typeof(TEntity).Name;

    protected async Task ExecuteRepositoryOperationInTransactionAsync(string operation, Func<ejemploDbContext, Task> action, string errorMessage)
    {
        var transaction = DbContext.Database.CurrentTransaction is null
            ? await DbContext.Database.BeginTransactionAsync()
            : null;

        try
        {
            await action(DbContext);
            await DbContext.SaveChangesAsync();

            if (transaction is not null)
            {
                await transaction.CommitAsync();
            }
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch (Exception rollbackException)
                {
                    Logger?.LogError(rollbackException, "Error rolling back {Entity} repository transaction.", EntityName);
                }
            }

            Logger?.LogError(ex, "{ErrorMessage}", errorMessage);
            throw new DataException(errorMessage, ex);
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    private static ejemploDbContext CreateDbContext()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DbConnection")
            ?? throw new InvalidOperationException("No application connection string provided.");

        var options = new DbContextOptionsBuilder<ejemploDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new ejemploDbContext(options);
    }
}