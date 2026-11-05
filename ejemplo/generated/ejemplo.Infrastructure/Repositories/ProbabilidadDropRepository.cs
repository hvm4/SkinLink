using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class ProbabilidadDropRepository : RepositoryBase<ProbabilidadDrop>, IProbabilidadDropRepository
{
    public ProbabilidadDropRepository(ILogger<ProbabilidadDropRepository>? logger = null)
        : base(logger)
    {
    }

    public ProbabilidadDropRepository(ejemploDbContext dbContext, ILogger<ProbabilidadDropRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<ProbabilidadDrop>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<ProbabilidadDrop>().Include(entity => entity.Caja).Include(entity => entity.ActivoRecompensa).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<ProbabilidadDrop?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<ProbabilidadDrop>().FindAsync([id]);
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Caja).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.ActivoRecompensa).LoadAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(ProbabilidadDrop entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<ProbabilidadDrop>();
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

    public async Task DeleteAsync(ProbabilidadDrop entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<ProbabilidadDrop>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(ProbabilidadDrop entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<ProbabilidadDrop> set, ProbabilidadDrop entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);

}