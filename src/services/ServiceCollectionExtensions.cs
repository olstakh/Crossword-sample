using CrossWords.Services;
using CrossWords.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPuzzleServices(this IServiceCollection services, string contentPath)
    {
        // Register cryptogram generator and crossword service
        var puzzlesDbPath = Path.Combine(contentPath, "Data", "puzzles.db");
        services.AddSingleton<SqlitePuzzleRepository>(sp => 
        {
            var logger = sp.GetRequiredService<ILogger<SqlitePuzzleRepository>>();
            return new SqlitePuzzleRepository(puzzlesDbPath, logger);
        });
        services.AddSingleton<IPuzzleRepository>(sp => sp.GetRequiredService<SqlitePuzzleRepository>());
        services.AddSingleton<IPuzzleRepositoryPersister>(sp => sp.GetRequiredService<SqlitePuzzleRepository>());
        services.AddSingleton<ICrosswordService, CrosswordService>();
        return services;
    }

    public static IServiceCollection AddUserServices(this IServiceCollection services, string contentPath)
    {
        // Register user progress repository and service
        var userProgressDbPath = Path.Combine(contentPath, "Data", "user-progress.db");
        services.AddSingleton<IUserProgressRepository>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SqliteUserProgressRepository>>();
            return new SqliteUserProgressRepository(userProgressDbPath, logger);
        });
        services.AddSingleton<IUserProgressService, UserProgressService>();

        return services;
    }
}