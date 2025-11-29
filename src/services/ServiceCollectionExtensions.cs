using CrossWords.Services;
using CrossWords.Services.Abstractions;
using CrossWords.Services.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add crossword services based on configuration
    /// This is the recommended method - it reads from appsettings.json
    /// </summary>
    public static IServiceCollection AddCrosswordServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string? contentRootPath = null)
    {
        var storageConfig = configuration
            .GetSection(StorageConfiguration.SectionName)
            .Get<StorageConfiguration>() ?? throw new ConfigurationSectionException(StorageConfiguration.SectionName);

        // Bind configuration for later use
        services.Configure<StorageConfiguration>(
            configuration.GetSection(StorageConfiguration.SectionName));

        // Register business logic services
        services.AddSingleton<ICrosswordService, CrosswordService>();

        // Register storage based on provider
        switch (storageConfig.Provider.ToLowerInvariant())
        {
            case "inmemory":
                services.AddInMemoryRepositories();
                break;

            case "sqlite":
                if (storageConfig.Sqlite == null)
                {
                    throw new InvalidOperationException(
                        "Sqlite configuration is required when Provider is 'Sqlite'");
                }

                var puzzlesPath = Path.IsPathRooted(storageConfig.Sqlite.PuzzlesDbPath)
                    ? storageConfig.Sqlite.PuzzlesDbPath
                    : Path.Combine(contentRootPath ?? Directory.GetCurrentDirectory(), 
                        storageConfig.Sqlite.PuzzlesDbPath);

                var userProgressPath = Path.IsPathRooted(storageConfig.Sqlite.UserProgressDbPath)
                    ? storageConfig.Sqlite.UserProgressDbPath
                    : Path.Combine(contentRootPath ?? Directory.GetCurrentDirectory(), 
                        storageConfig.Sqlite.UserProgressDbPath);

                services.AddSqlitePuzzleRepository(puzzlesPath);
                services.AddSqliteUserProgressRepository(userProgressPath);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown storage provider: {storageConfig.Provider}. " +
                    $"Supported providers: InMemory, Sqlite, SqlServer");
        }

        return services;
    }

    #region Individual Repository Registration Methods
    
    /// <summary>
    /// Add in-memory repositories (for testing or development)
    /// </summary>
    internal static IServiceCollection AddInMemoryRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IPuzzleRepositoryReader, Testing.InMemoryPuzzleRepository>();
        services.AddSingleton<IPuzzleRepositoryWriter, Testing.InMemoryPuzzleRepository>();
        services.AddSingleton<IUserProgressRepositoryReader, Testing.InMemoryUserProgressRepository>();
        services.AddSingleton<IUserProgressRepositoryWriter, Testing.InMemoryUserProgressRepository>();
        return services;
    }

    /// <summary>
    /// Add SQLite-based puzzle repository
    /// </summary>
    internal static IServiceCollection AddSqlitePuzzleRepository(this IServiceCollection services, string dbFilePath)
    {
        services.AddSingleton<SqlitePuzzleRepository>(sp => 
        {
            var logger = sp.GetRequiredService<ILogger<SqlitePuzzleRepository>>();
            return new SqlitePuzzleRepository(dbFilePath, logger);
        });
        services.AddSingleton<IPuzzleRepositoryReader>(sp => sp.GetRequiredService<SqlitePuzzleRepository>());
        services.AddSingleton<IPuzzleRepositoryWriter>(sp => sp.GetRequiredService<SqlitePuzzleRepository>());
        return services;
    }

    /// <summary>
    /// Add SQLite-based user progress repository
    /// </summary>
    internal static IServiceCollection AddSqliteUserProgressRepository(this IServiceCollection services, string dbFilePath)
    {
        services.AddSingleton<SqliteUserProgressRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SqliteUserProgressRepository>>();
            return new SqliteUserProgressRepository(dbFilePath, logger);
        });

        services.AddSingleton<IUserProgressRepositoryReader>(sp => sp.GetRequiredService<SqliteUserProgressRepository>());
        services.AddSingleton<IUserProgressRepositoryWriter>(sp => sp.GetRequiredService<SqliteUserProgressRepository>());
        
        return services;
    }

    #endregion
}