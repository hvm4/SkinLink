using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class IntercambioRepository : RepositoryBase<Intercambio>, IIntercambioRepository
{
    public IntercambioRepository(ILogger<IntercambioRepository>? logger = null)
        : base(logger)
    {
    }

    public IntercambioRepository(ejemploDbContext dbContext, ILogger<IntercambioRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<Intercambio>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<Intercambio>().Include(entity => entity.Proponente).Include(entity => entity.Receptor).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Intercambio?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<Intercambio>().FindAsync([id]);
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Proponente).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Receptor).LoadAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(Intercambio entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<Intercambio>();
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

    public async Task DeleteAsync(Intercambio entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<Intercambio>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(Intercambio entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<Intercambio> set, Intercambio entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);

}