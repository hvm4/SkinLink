using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class ActivoDigitalRepository : RepositoryBase<ActivoDigital>, IActivoDigitalRepository
{
    public ActivoDigitalRepository(ILogger<ActivoDigitalRepository>? logger = null)
        : base(logger)
    {
    }

    public ActivoDigitalRepository(ejemploDbContext dbContext, ILogger<ActivoDigitalRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<ActivoDigital>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<ActivoDigital>().Include(entity => entity.Inventario).Include(entity => entity.Venta).Include(entity => entity.IntercambioOfrecido).Include(entity => entity.IntercambioSolicitado).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<ActivoDigital?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<ActivoDigital>().FindAsync([id]);
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Inventario).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Venta).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.IntercambioOfrecido).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.IntercambioSolicitado).LoadAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(ActivoDigital entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<ActivoDigital>();
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

    public async Task DeleteAsync(ActivoDigital entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<ActivoDigital>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(ActivoDigital entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<ActivoDigital> set, ActivoDigital entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);
    public async Task<IReadOnlyList<ActivoDigital>> FiltrarActivosPorRarezaAsync(Rareza p_rareza)
    {
        try
        {
            return await DbContext.Set<ActivoDigital>()
                .Where(entity => entity.Atributos != null && entity.Atributos.Rareza == p_rareza)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter ActivoDigital.filtrarActivosPorRareza.");
            throw new DataException("Error executing ReadFilter ActivoDigital.filtrarActivosPorRareza.", ex);
        }
    }

    public async Task<IReadOnlyList<ActivoDigital>> FiltrarActivosPorColeccionAsync(string p_coleccion)
    {
        try
        {
            return await DbContext.Set<ActivoDigital>()
                .Where(entity => entity.Coleccion == p_coleccion)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter ActivoDigital.filtrarActivosPorColeccion.");
            throw new DataException("Error executing ReadFilter ActivoDigital.filtrarActivosPorColeccion.", ex);
        }
    }

    public async Task<IReadOnlyList<ActivoDigital>> FiltrarActivosPorCondicionAsync(Condicion p_condicion)
    {
        try
        {
            return await DbContext.Set<ActivoDigital>()
                .Where(entity => entity.Atributos != null && entity.Atributos.Condicion == p_condicion)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter ActivoDigital.filtrarActivosPorCondicion.");
            throw new DataException("Error executing ReadFilter ActivoDigital.filtrarActivosPorCondicion.", ex);
        }
    }

    public async Task<IReadOnlyList<ActivoDigital>> FiltrarActivosEnVentaAsync(int p_usuarioId, bool p_enVenta)
    {
        try
        {
            return await DbContext.Set<ActivoDigital>()
                .Where(entity => entity.Inventario.Usuario.Id == p_usuarioId && entity.EnVenta == p_enVenta)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter ActivoDigital.filtrarActivosEnVenta.");
            throw new DataException("Error executing ReadFilter ActivoDigital.filtrarActivosEnVenta.", ex);
        }
    }
}