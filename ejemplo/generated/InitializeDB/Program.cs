using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ejemplo.Infrastructure.Data;
using ejemplo.Infrastructure.Logging;

namespace InitializeDB;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("-----------------------------------------------------------------------------");
        Console.WriteLine("A new database called: ejemploEntityFramework will be created (the previous information will be deleted).");
        Console.WriteLine("-----------------------------------------------------------------------------");

        try
        {
            var config = BuildConfiguration();
            using var loggerFactory = CreateLoggerFactory(config);
            var databaseName = config["Database:Name"] ?? "ejemploEntityFramework";

            var cs = config.GetConnectionString("DbConnection")
                ?? throw new InvalidOperationException("No application connection string provided.");
            cs = ResolveSqliteConnectionString(cs);

            var options = new DbContextOptionsBuilder<ejemploDbContext>()
            .UseSqlite(cs)
                .UseLoggerFactory(loggerFactory)
                .Options;

            Console.WriteLine($"Target database: {databaseName}");
            await CreateDB.RecreateDatabaseAsync(options);
            await CreateDB.CreateSchemaAsync(options);

            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Database schema created successfully");
            Console.WriteLine("-------------------------------------");

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("Let's initialize the data of your database!");
            Console.WriteLine("-------------------------------------------------------");

            await SeedData.InitializeDataAsync(options, loggerFactory);
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("The data has been inserted successfully!!");
            Console.WriteLine("-----------------------------------------");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static string ResolveSqliteConnectionString(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (!string.IsNullOrWhiteSpace(builder.DataSource) &&
            !Path.IsPathRooted(builder.DataSource) &&
            !string.Equals(builder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase) &&
            !builder.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(GetInitializeDbDirectory(), builder.DataSource));
        }

        return builder.ConnectionString;
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

    private static ILoggerFactory CreateLoggerFactory(IConfiguration configuration)
    {
        var minimumLevel = Enum.TryParse<LogLevel>(configuration["Logging:File:MinimumLevel"], ignoreCase: true, out var parsedLevel)
            ? parsedLevel
            : LogLevel.Information;

        var configuredPath = configuration["Logging:File:Path"];
        var filePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(GetInitializeDbDirectory(), "logs", "application.log")
            : ResolveLogPath(configuredPath);

        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(minimumLevel);
            builder.AddProvider(new FileLoggerProvider(new FileLoggerOptions
            {
                FilePath = filePath,
                MinimumLevel = minimumLevel
            }));
        });
    }

    private static string ResolveLogPath(string configuredPath) =>
        Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(GetInitializeDbDirectory(), configuredPath);

    private static string GetInitializeDbDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "InitializeDB.csproj")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}