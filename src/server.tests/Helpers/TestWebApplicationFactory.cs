using CrossWords.Services.Abstractions;
using CrossWords.Services.Models;
using CrossWords.Services.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CrossWords.Tests.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration tests with in-memory storage
/// Uses appsettings.Testing.json configuration and seeds test data
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use Testing environment
        builder.UseEnvironment("Testing");

        // Override configuration to use InMemory storage for tests
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test-specific configuration that overrides server's appsettings
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Provider"] = "InMemory"
            });
        });

        // Replace in-memory repository registration with one that has seeded data
        builder.ConfigureServices((context, services) =>
        {
            // Create sample puzzles for testing
            var testPuzzles = new List<CrosswordPuzzle>
            {
                new CrosswordPuzzle
                {
                    Id = "puzzle1",
                    Title = "Test Small Puzzle",
                    Language = PuzzleLanguage.English,
                    Size = new PuzzleSize { Rows = 5, Cols = 5 },
                    Grid = new List<List<string>>
                    {
                        new List<string> { "C", "A", "T", "#", "#" },
                        new List<string> { "A", "#", "E", "#", "#" },
                        new List<string> { "R", "#", "S", "#", "#" },
                        new List<string> { "#", "#", "T", "#", "#" },
                        new List<string> { "#", "#", "#", "#", "#" }
                    }
                },
                new CrosswordPuzzle
                {
                    Id = "puzzle2",
                    Title = "Test Medium Puzzle",
                    Language = PuzzleLanguage.English,
                    Size = new PuzzleSize { Rows = 10, Cols = 10 },
                    Grid = Enumerable.Range(0, 10)
                        .Select(_ => Enumerable.Range(0, 10).Select(__ => "A").ToList())
                        .ToList()
                },
                new CrosswordPuzzle
                {
                    Id = "puzzle3",
                    Title = "Test Big Puzzle (English)",
                    Language = PuzzleLanguage.English,
                    Size = new PuzzleSize { Rows = 17, Cols = 17 },
                    Grid = Enumerable.Range(0, 17)
                        .Select(_ => Enumerable.Range(0, 17).Select(__ => "A").ToList())
                        .ToList()
                },
                new CrosswordPuzzle
                {
                    Id = "puzzle4",
                    Title = "Test Medium Puzzle (Ukrainian)",
                    Language = PuzzleLanguage.Ukrainian,
                    Size = new PuzzleSize { Rows = 11, Cols = 11 },
                    Grid = Enumerable.Range(0, 11)
                        .Select(_ => Enumerable.Range(0, 11).Select(__ => "А").ToList())
                        .ToList()
                },
                new CrosswordPuzzle
                {
                    Id = "puzzle5",
                    Title = "Test Big Puzzle (Ukrainian)",
                    Language = PuzzleLanguage.Ukrainian,
                    Size = new PuzzleSize { Rows = 18, Cols = 18 },
                    Grid = Enumerable.Range(0, 18)
                        .Select(_ => Enumerable.Range(0, 18).Select(__ => "А").ToList())
                        .ToList()
                }
            };

            // Remove the default in-memory repository registrations
            services.RemoveAll<IPuzzleRepositoryReader>();
            services.RemoveAll<IPuzzleRepositoryWriter>();

            // Register with initial puzzles
            services.AddSingleton<IPuzzleRepositoryReader>(sp => 
                new InMemoryPuzzleRepository(testPuzzles));
            services.AddSingleton<IPuzzleRepositoryWriter>(sp => 
                (InMemoryPuzzleRepository)sp.GetRequiredService<IPuzzleRepositoryReader>());
        });
    }
}

/// <summary>
/// Example usage in tests:
/// 
/// public class CrosswordApiTests : IClassFixture<TestWebApplicationFactory>
/// {
///     private readonly HttpClient _client;
///     
///     public CrosswordApiTests(TestWebApplicationFactory factory)
///     {
///         _client = factory.CreateClient();
///     }
///     
///     [Fact]
///     public async Task GetPuzzle_ReturnsSuccess()
///     {
///         var response = await _client.GetAsync("/api/crossword/puzzle?size=Medium");
///         response.EnsureSuccessStatusCode();
///     }
/// }
/// </summary>
/// 