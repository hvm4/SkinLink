using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class TransaccionRepository : RepositoryBase<Transaccion>, ITransaccionRepository
{
    public TransaccionRepository(ILogger<TransaccionRepository>? logger = null)
        : base(logger)
    {
    }

    public TransaccionRepository(ejemploDbContext dbContext, ILogger<TransaccionRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<Transaccion>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<Transaccion>().Include(entity => entity.Monedero).Include(entity => entity.VentaCobro).Include(entity => entity.VentaPago).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Transaccion?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<Transaccion>().FindAsync([id]);
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.Monedero).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.VentaCobro).LoadAsync();
                if (entity is not null) await DbContext.Entry(entity).Reference(item => item.VentaPago).LoadAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(Transaccion entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<Transaccion>();
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

    public async Task DeleteAsync(Transaccion entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<Transaccion>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(Transaccion entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<Transaccion> set, Transaccion entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);
    public async Task<IReadOnlyList<Transaccion>> FiltrarTransaccionesPorTipoAsync(TipoTransaccion p_tipo)
    {
        try
        {
            return await DbContext.Set<Transaccion>()
                .Where(entity => entity.Tipo == p_tipo)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Transaccion.filtrarTransaccionesPorTipo.");
            throw new DataException("Error executing ReadFilter Transaccion.filtrarTransaccionesPorTipo.", ex);
        }
    }

    public async Task<IReadOnlyList<Transaccion>> FiltrarTransaccionesPorRangoFechasAsync(DateTime p_fechaInicio, DateTime p_fechaFin)
    {
        try
        {
            return await DbContext.Set<Transaccion>()
                .Where(entity => entity.Fecha >= p_fechaInicio && entity.Fecha <= p_fechaFin)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Transaccion.filtrarTransaccionesPorRangoFechas.");
            throw new DataException("Error executing ReadFilter Transaccion.filtrarTransaccionesPorRangoFechas.", ex);
        }
    }
}