using Microsoft.EntityFrameworkCore;
using ejemplo.Infrastructure.Data;

namespace InitializeDB;

public static class CreateDB
{
    public static async Task RecreateDatabaseAsync(DbContextOptions<ejemploDbContext> options)
    {
        await using var db = new ejemploDbContext(options);
        await db.Database.EnsureDeletedAsync();
    }

    public static async Task CreateSchemaAsync(DbContextOptions<ejemploDbContext> options)
    {
        await using var db = new ejemploDbContext(options);
        await db.Database.EnsureCreatedAsync();
    }
}