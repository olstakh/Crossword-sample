using CrossWords.Services.Abstractions;
using CrossWords.Services.Models;
using CrossWords.Services.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CrossWords.Services.Extensions;

/// <summary>
/// Service collection extensions for test scenarios
/// Provides in-memory implementations that are isolated per test
/// </summary>
public static class TestingServiceCollectionExtensions
{
    /// <summary>
    /// Add in-memory repositories for testing with optional seed data
    /// Each call creates a NEW isolated instance - perfect for parallel tests
    /// </summary>
    public static IServiceCollection AddInMemoryRepositories(
        this IServiceCollection services,
        IEnumerable<CrosswordPuzzle>? seedPuzzles = null)
    {
        // Create isolated instances for this test
        var puzzleRepo = new InMemoryPuzzleRepository(seedPuzzles ?? Enumerable.Empty<CrosswordPuzzle>());
        var userRepo = new InMemoryUserProgressRepository();

        services.AddSingleton<IPuzzleRepository>(puzzleRepo);
        services.AddSingleton<IPuzzleRepositoryPersister>(puzzleRepo);
        services.AddSingleton<IUserProgressRepository>(userRepo);

        return services;
    }

    /// <summary>
    /// Convenience method: Add all services with in-memory repositories for testing
    /// </summary>
    public static IServiceCollection AddCrosswordServicesWithInMemory(
        this IServiceCollection services,
        IEnumerable<CrosswordPuzzle>? seedPuzzles = null)
    {
        return services
            .AddInMemoryRepositories(seedPuzzles)
            .AddCrosswordServices();
    }
}
