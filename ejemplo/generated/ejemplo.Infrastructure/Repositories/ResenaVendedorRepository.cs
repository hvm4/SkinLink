using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class ResenaVendedorRepository : RepositoryBase<ResenaVendedor>, IResenaVendedorRepository
{
    public ResenaVendedorRepository(ILogger<ResenaVendedorRepository>? logger = null)
        : base(logger)
    {
    }

    public ResenaVendedorRepository(ejemploDbContext dbContext, ILogger<ResenaVendedorRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<ResenaVendedor>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<ResenaVendedor>().Include(entity => entity.Comprador).Include(entity => entity.Vendedor).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<ResenaVendedor?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<ResenaVendedor>().FindAsync([id]);
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Comprador).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Vendedor).LoadAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(ResenaVendedor entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<ResenaVendedor>();
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

    public async Task DeleteAsync(ResenaVendedor entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<ResenaVendedor>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(ResenaVendedor entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<ResenaVendedor> set, ResenaVendedor entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);
    public async Task<IReadOnlyList<ResenaVendedor>> FiltrarResenasVisiblesPorVendedorAsync(int p_vendedorId)
    {
        try
        {
            return await DbContext.Set<ResenaVendedor>()
                .Where(entity => entity.Vendedor.Id == p_vendedorId && entity.Oculta == false)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter ResenaVendedor.filtrarResenasVisiblesPorVendedor.");
            throw new DataException("Error executing ReadFilter ResenaVendedor.filtrarResenasVisiblesPorVendedor.", ex);
        }
    }
}