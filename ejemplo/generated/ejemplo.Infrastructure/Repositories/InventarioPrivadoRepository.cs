using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class InventarioPrivadoRepository : RepositoryBase<InventarioPrivado>, IInventarioPrivadoRepository
{
    public InventarioPrivadoRepository(ILogger<InventarioPrivadoRepository>? logger = null)
        : base(logger)
    {
    }

    public InventarioPrivadoRepository(ejemploDbContext dbContext, ILogger<InventarioPrivadoRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<InventarioPrivado>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<InventarioPrivado>().Include(entity => entity.Usuario).Include(entity => entity.Activos).Include("Activos.Venta").Include("Activos.IntercambioOfrecido").Include("Activos.IntercambioSolicitado").ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<InventarioPrivado?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<InventarioPrivado>().FindAsync([id]);
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Usuario).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Collection(item => item.Activos).LoadAsync();
                if (entity is not null) foreach (var child in entity.Activos) await DbContext.Entry(child).Reference(item => item.Venta).LoadAsync();
                if (entity is not null) foreach (var child in entity.Activos) await DbContext.Entry(child).Reference(item => item.IntercambioOfrecido).LoadAsync();
                if (entity is not null) foreach (var child in entity.Activos) await DbContext.Entry(child).Reference(item => item.IntercambioSolicitado).LoadAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(InventarioPrivado entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<InventarioPrivado>();
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

    public async Task DeleteAsync(InventarioPrivado entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<InventarioPrivado>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(InventarioPrivado entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<InventarioPrivado> set, InventarioPrivado entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);

}