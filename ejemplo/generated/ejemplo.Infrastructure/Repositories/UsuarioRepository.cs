using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ejemplo.Application.Abstractions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;
using ejemplo.Domain.ValueObjects;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Exceptions;

namespace ejemplo.Infrastructure.Repositories;

public sealed class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(ILogger<UsuarioRepository>? logger = null)
        : base(logger)
    {
    }

    public UsuarioRepository(ejemploDbContext dbContext, ILogger<UsuarioRepository>? logger = null)
        : base(dbContext, logger)
    {
    }

    public async Task<IReadOnlyList<Usuario>> ReadAllAsync()
    {
        try
        {
            return await DbContext.Set<Usuario>().Include(entity => entity.Monedero).Include(entity => entity.Inventario).ToListAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error reading all {Entity} entities.", EntityName);
            throw new DataException($"Error reading all {EntityName} entities.", ex);
        }
    }

    public async Task<Usuario?> ReadAsync(object id)
    {
        try
        {
            var entity = await DbContext.Set<Usuario>().FindAsync([id]);
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

    public async Task SaveAsync(Usuario entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Save",
            async dbContext =>
            {
                var set = dbContext.Set<Usuario>();
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

    public async Task DeleteAsync(Usuario entity)
    {
        await ExecuteRepositoryOperationInTransactionAsync(
            "Delete",
            dbContext =>
            {
                dbContext.Set<Usuario>().Remove(entity);
                return Task.CompletedTask;
            },
            $"Error deleting {EntityName} entity.");
    }

    private static bool HasDefaultKey(Usuario entity) =>
        EqualityComparer<int>.Default.Equals(entity.Id, default);

    private static Task<bool> EntityExistsAsync(DbSet<Usuario> set, Usuario entity) =>
        set.AsNoTracking().AnyAsync(existing => existing.Id == entity.Id);
    public async Task<IReadOnlyList<Usuario>> FiltrarUsuariosPorEstadoAsync(EstadoCuenta p_estado)
    {
        try
        {
            return await DbContext.Set<Usuario>()
                .Where(entity => entity.Estado == p_estado)
                .ToListAsync();
        }
        catch (DataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error executing ReadFilter Usuario.filtrarUsuariosPorEstado.");
            throw new DataException("Error executing ReadFilter Usuario.filtrarUsuariosPorEstado.", ex);
        }
    }
}