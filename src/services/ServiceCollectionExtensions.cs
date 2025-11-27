using CrossWords.Services;
using CrossWords.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add core crossword services (business logic)
    /// Use this for shared services regardless of storage implementation
    /// </summary>
    public static IServiceCollection AddCrosswordServices(this IServiceCollection services)
    {
        services.AddSingleton<ICrosswordService, CrosswordService>();
        services.AddSingleton<IUserProgressService, UserProgressService>();
        return services;
    }

    /// <summary>
    /// Add SQLite-based puzzle repository
    /// Use this for production or when persistent storage is needed
    /// </summary>
    public static IServiceCollection AddSqlitePuzzleRepository(this IServiceCollection services, string dbFilePath)
    {
        services.AddSingleton<SqlitePuzzleRepository>(sp => 
        {
            var logger = sp.GetRequiredService<ILogger<SqlitePuzzleRepository>>();
            return new SqlitePuzzleRepository(dbFilePath, logger);
        });
        services.AddSingleton<IPuzzleRepository>(sp => sp.GetRequiredService<SqlitePuzzleRepository>());
        services.AddSingleton<IPuzzleRepositoryPersister>(sp => sp.GetRequiredService<SqlitePuzzleRepository>());
        return services;
    }

    /// <summary>
    /// Add SQLite-based user progress repository
    /// Use this for production or when persistent storage is needed
    /// </summary>
    public static IServiceCollection AddSqliteUserProgressRepository(this IServiceCollection services, string dbFilePath)
    {
        services.AddSingleton<IUserProgressRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SqliteUserProgressRepository>>();
            return new SqliteUserProgressRepository(dbFilePath, logger);
        });
        return services;
    }

    /// <summary>
    /// Convenience method: Add all services with SQLite repositories for production use
    /// </summary>
    public static IServiceCollection AddCrosswordServicesWithSqlite(
        this IServiceCollection services, 
        string contentPath)
    {
        var puzzlesDbPath = Path.Combine(contentPath, "Data", "puzzles.db");
        var userProgressDbPath = Path.Combine(contentPath, "Data", "user-progress.db");

        return services
            .AddSqlitePuzzleRepository(puzzlesDbPath)
            .AddSqliteUserProgressRepository(userProgressDbPath)
            .AddCrosswordServices();
    }

    /// <summary>
    /// Add in-memory puzzle repository for testing
    /// Caller provides the implementation instance or factory
    /// </summary>
    public static IServiceCollection AddPuzzleRepository<TRepository>(
        this IServiceCollection services,
        Func<IServiceProvider, TRepository> implementationFactory)
        where TRepository : class, IPuzzleRepository
    {
        services.AddSingleton<IPuzzleRepository>(implementationFactory);
        
        // If it also implements persister, register that too
        if (typeof(IPuzzleRepositoryPersister).IsAssignableFrom(typeof(TRepository)))
        {
            services.AddSingleton<IPuzzleRepositoryPersister>(sp => 
                (IPuzzleRepositoryPersister)sp.GetRequiredService<IPuzzleRepository>());
        }
        
        return services;
    }

    /// <summary>
    /// Add user progress repository with custom implementation
    /// </summary>
    public static IServiceCollection AddUserProgressRepository<TRepository>(
        this IServiceCollection services,
        Func<IServiceProvider, TRepository> implementationFactory)
        where TRepository : class, IUserProgressRepository
    {
        services.AddSingleton<IUserProgressRepository>(implementationFactory);
        return services;
    }
}