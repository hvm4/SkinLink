using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class ArticuloCatalogoRepository : RepositoryBase<ArticuloCatalogo>, IArticuloCatalogoRepository
{
    public ArticuloCatalogoRepository(ILogger<ArticuloCatalogoRepository>? logger = null)
        : base(logger)
    {
    }

    public ArticuloCatalogoRepository(ejemploDbContext dbContext, ILogger<ArticuloCatalogoRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<ArticuloCatalogo>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<ArticuloCatalogo>().ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<ArticuloCatalogo?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<ArticuloCatalogo>().FindAsync([id]);

            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(ArticuloCatalogo entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<ArticuloCatalogo>();
                var entry = dbContext.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    if (HasDefaultKey(entity))
                    {
                        set.Add(entity);
                    }
                    else
                    {
                        var exists = await EntityExistsAsync(set, entity);
                        if (exists)
                        {
                            set.Update(entity);
                        }
                        else
                        {
                            set.Add(entity);
                        }
                    }
                }
                else
                {
                    set.Update(entity);
                }
            },
            $"Error saving {EntityName} entity.");
    }

    public async Task DeleteAsync(ArticuloCatalogo entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<ArticuloCatalogo>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(ArticuloCatalogo entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<ArticuloCatalogo> set, ArticuloCatalogo entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);

}