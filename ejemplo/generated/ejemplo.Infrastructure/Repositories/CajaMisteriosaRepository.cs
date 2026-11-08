using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class CajaMisteriosaRepository : RepositoryBase<CajaMisteriosa>, ICajaMisteriosaRepository
{
    public CajaMisteriosaRepository(ILogger<CajaMisteriosaRepository>? logger = null)
        : base(logger)
    {
    }

    public CajaMisteriosaRepository(ejemploDbContext dbContext, ILogger<CajaMisteriosaRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<CajaMisteriosa>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<CajaMisteriosa>().Include(entity => entity.Probabilidades).Include("Probabilidades.ActivoRecompensa").ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<CajaMisteriosa?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<CajaMisteriosa>().FindAsync([id]);
                if (entity is not null) await DbContext.Entry(entity).Collection(item => item.Probabilidades).LoadAsync();
                if (entity is not null) foreach (var child in entity.Probabilidades) await DbContext.Entry(child).Reference(item => item.ActivoRecompensa).LoadAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading {Entity} entity with id {EntityId}.", EntityName, id);
            throw new DataException($"Error reading {EntityName} entity.", ex);
        }
    }

    public async Task SaveAsync(CajaMisteriosa entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<CajaMisteriosa>();
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

    public async Task DeleteAsync(CajaMisteriosa entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<CajaMisteriosa>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(CajaMisteriosa entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<CajaMisteriosa> set, CajaMisteriosa entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);
    public async Task<IReadOnlyList<CajaMisteriosa>> FiltrarCajasActivasAsync()
    {
        try
        {
            return await DbContext.Set<CajaMisteriosa>()
                .Where(entity => entity.Activa == true)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter CajaMisteriosa.filtrarCajasActivas.");
            throw new DataException("Error executing ReadFilter CajaMisteriosa.filtrarCajasActivas.", ex);
        }
    }

    public async Task<IReadOnlyList<CajaMisteriosa>> FiltrarCajasPorPrecioAperturaAsync(double p_precioMinimo, double p_precioMaximo)
    {
        try
        {
            return await DbContext.Set<CajaMisteriosa>()
                .Where(entity => (double)entity.PrecioApertura.Valor >= p_precioMinimo && (double)entity.PrecioApertura.Valor <= p_precioMaximo)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter CajaMisteriosa.filtrarCajasPorPrecioApertura.");
            throw new DataException("Error executing ReadFilter CajaMisteriosa.filtrarCajasPorPrecioApertura.", ex);
        }
    }
}