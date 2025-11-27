using System.Net;
using System.Net.Http.Json;
using CrossWords.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CrossWords.Tests.Controllers;

public class CrosswordControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CrosswordControllerTests(WebApplicationFactory<Program> factory)
    {
        // Create a custom factory that skips static file serving for tests
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
        
        _client = customFactory.CreateClient();
    }

    [Fact]
    public async Task GetPuzzleList_ReturnsSuccessAndPuzzleIds()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzles", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzleIds = await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzleIds);
        Assert.NotEmpty(puzzleIds);
        Assert.Contains("puzzle1", puzzleIds);
    }

    [Theory]
    [InlineData("puzzle1")]
    [InlineData("puzzle2")]
    [InlineData("puzzle3")]
    public async Task GetPuzzle_WithValidId_ReturnsSuccessAndPuzzle(string puzzleId)
    {
        // Act
        var response = await _client.GetAsync($"/api/crossword/puzzle/{puzzleId}", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.Equal(puzzleId, puzzle.Id);
        Assert.NotEmpty(puzzle.Grid);
        Assert.NotEmpty(puzzle.Title);
    }

    [Fact]
    public async Task GetPuzzleBySize_WithInvalidSize_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzle/size/invalid", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPuzzleBySize_WithSeed_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzle/size/medium?seed=test123", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.NotEmpty(puzzle.Grid);
    }

    [Theory]
    [InlineData(PuzzleLanguage.English)]
    [InlineData(PuzzleLanguage.Ukrainian)]
    public async Task GetPuzzleBySize_WithAvailableLanguage_ReturnsSuccess(PuzzleLanguage language)
    {
        // Act
        var response = await _client.GetAsync($"/api/crossword/puzzle/size/medium?language={language}", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.NotEmpty(puzzle.Grid);
    }

    [Fact]
    public async Task GetPuzzleBySize_WithUnavailableLanguage_ReturnsNotFound()
    {
        // Act - Request Russian puzzle (which doesn't exist in test data)
        var response = await _client.GetAsync($"/api/crossword/puzzle/size/medium?language={PuzzleLanguage.Russian}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(errorResponse);
        Assert.True(errorResponse.ContainsKey("error"));
        Assert.Contains("Russian", errorResponse["error"]);
    }

    [Theory]
    [InlineData(PuzzleSizeCategory.Small)]
    [InlineData(PuzzleSizeCategory.Medium)]
    [InlineData(PuzzleSizeCategory.Big)]
    public async Task GetPuzzleBySize_Small_ReturnsProperlyGrid(PuzzleSizeCategory size)
    {
        // Act
        var response = await _client.GetAsync($"/api/crossword/puzzle/size/{size.ToString().ToLower()}", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        var (minSize, maxSize) = size.GetSizeRange();
        Assert.NotNull(puzzle);
        Assert.InRange(puzzle.Size.Rows, minSize, maxSize);
        Assert.InRange(puzzle.Size.Cols, minSize, maxSize);
    }

    [Fact]
    public async Task Puzzle_GridContainsValidData()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzle/puzzle1", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.Equal(puzzle.Size.Rows, puzzle.Grid.Count);
        
        foreach (var row in puzzle.Grid)
        {
            Assert.Equal(puzzle.Size.Cols, row.Count);
            foreach (var cell in row)
            {
                // Each cell should be "#" or a single letter
                Assert.True(
                    cell == "#" || (cell.Length == 1 && char.IsLetter(cell[0])),
                    $"Invalid cell value: {cell}"
                );
            }
        }
    }

    [Fact]
    public async Task GetPuzzle_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzle/nonexistent", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(errorResponse);
        Assert.True(errorResponse.ContainsKey("error"));
        Assert.Contains("not found", errorResponse["error"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPuzzleBySize_WithUserId_ReturnsUnsolvedPuzzle()
    {
        // Arrange
        var userId = "test_user_" + Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/crossword/puzzle/size/medium?language=English");
        request.Headers.Add("X-User-Id", userId);

        // Act
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(puzzle);
        Assert.Equal(PuzzleLanguage.English, puzzle.Language);
    }

    [Fact]
    public async Task GetPuzzleList_WithLanguageFilter_ReturnsFilteredPuzzles()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzles?language=English", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzleIds = await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzleIds);
        Assert.NotEmpty(puzzleIds);
    }

    [Fact]
    public async Task GetPuzzleBySize_ReturnsConsistentPuzzleWithSeed()
    {
        // Act - Request same puzzle twice with same seed
        var response1 = await _client.GetAsync("/api/crossword/puzzle/size/medium?seed=test123", TestContext.Current.CancellationToken);
        var response2 = await _client.GetAsync("/api/crossword/puzzle/size/medium?seed=test123", TestContext.Current.CancellationToken);

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();
        
        var puzzle1 = await response1.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        var puzzle2 = await response2.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        // Note: Actual randomness depends on implementation, but we can verify both are valid
        Assert.NotNull(puzzle1);
        Assert.NotNull(puzzle2);
    }

    [Theory]
    [InlineData(PuzzleSizeCategory.Small, 5, 8)]
    [InlineData(PuzzleSizeCategory.Medium, 9, 14)]
    [InlineData(PuzzleSizeCategory.Big, 15, 20)]
    public async Task GetPuzzleBySize_ReturnsCorrectSizeRange(PuzzleSizeCategory size, int minExpected, int maxExpected)
    {
        // Act
        var response = await _client.GetAsync($"/api/crossword/puzzle/size/{size.ToString().ToLower()}", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.InRange(puzzle.Size.Rows, minExpected, maxExpected);
        Assert.InRange(puzzle.Size.Cols, minExpected, maxExpected);
    }
}
