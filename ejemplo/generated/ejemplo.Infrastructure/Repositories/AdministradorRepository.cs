using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class AdministradorRepository : RepositoryBase<Administrador>, IAdministradorRepository
{
    public AdministradorRepository(ILogger<AdministradorRepository>? logger = null)
        : base(logger)
    {
    }

    public AdministradorRepository(ejemploDbContext dbContext, ILogger<AdministradorRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<Administrador>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<Administrador>().Include(entity => entity.Monedero).Include(entity => entity.Inventario).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Administrador?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<Administrador>().FindAsync([id]);
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Monedero).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Inventario).LoadAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(Administrador entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<Administrador>();
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

    public async Task DeleteAsync(Administrador entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<Administrador>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(Administrador entity) => false;

    private static Task<bool> EntityExistsAsync(DbSet<Administrador> set, Administrador entity) =>
        Task.FromResult(true);

}