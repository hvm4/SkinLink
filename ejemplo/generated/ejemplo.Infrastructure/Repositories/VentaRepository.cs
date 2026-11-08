using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class VentaRepository : RepositoryBase<Venta>, IVentaRepository
{
    public VentaRepository(ILogger<VentaRepository>? logger = null)
        : base(logger)
    {
    }

    public VentaRepository(ejemploDbContext dbContext, ILogger<VentaRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<Venta>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<Venta>().Include(entity => entity.Vendedor).Include(entity => entity.Comprador).Include(entity => entity.ActivoDigital).Include(entity => entity.TransaccionCobro).Include(entity => entity.TransaccionPago).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Venta?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<Venta>().FindAsync([id]);
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Vendedor).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Comprador).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.ActivoDigital).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.TransaccionCobro).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.TransaccionPago).LoadAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(Venta entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<Venta>();
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

    public async Task DeleteAsync(Venta entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<Venta>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(Venta entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<Venta> set, Venta entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);
    public async Task<IReadOnlyList<Venta>> FiltrarVentasPorEstadoAsync(EstadoVenta p_estado)
    {
        try
        {
            return await DbContext.Set<Venta>()
                .Where(entity => entity.EstadoVenta == p_estado)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Venta.filtrarVentasPorEstado.");
            throw new DataException("Error executing ReadFilter Venta.filtrarVentasPorEstado.", ex);
        }
    }

    public async Task<IReadOnlyList<Venta>> FiltrarVentasPorPrecioMaximoAsync(double p_precioMaximo)
    {
        try
        {
            return await DbContext.Set<Venta>()
                .Where(entity => (double)entity.PrecioVenta.Valor <= p_precioMaximo)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Venta.filtrarVentasPorPrecioMaximo.");
            throw new DataException("Error executing ReadFilter Venta.filtrarVentasPorPrecioMaximo.", ex);
        }
    }
}