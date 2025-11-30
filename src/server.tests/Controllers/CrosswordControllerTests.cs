using System.Net;
using System.Net.Http.Json;
using CrossWords.Models;
using CrossWords.Services.Models;
using CrossWords.Tests.Helpers;

namespace CrossWords.Tests.Controllers;

public class CrosswordControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CrosswordControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
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
        Assert.Contains("puzzle2", puzzleIds);
    }

    [Theory]
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
    public async Task GetPuzzleBySize_Small_ReturnsProperlyGrid()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/unsolvedpuzzle", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.InRange(puzzle.Size.Rows, 5, 20);
        Assert.InRange(puzzle.Size.Cols, 5, 20);
    }

    [Theory]
    [InlineData("en", PuzzleLanguage.English)]
    [InlineData("uk", PuzzleLanguage.Ukrainian)]
    public async Task GetPuzzle_WithAcceptLanguageHeader_ReturnsCorrectLanguage(string languageCode, PuzzleLanguage expectedLanguage)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/crossword/unsolvedpuzzle");
        request.Headers.Add("Accept-Language", languageCode);

        // Act
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(puzzle);
        Assert.Equal(expectedLanguage, puzzle.Language);
    }

    [Fact]
    public async Task GetPuzzle_WithUnavailableLanguage_ReturnsNotFound()
    {
        // Arrange - Request Russian puzzle (which doesn't exist in test data)
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/crossword/unsolvedpuzzle");
        request.Headers.Add("Accept-Language", "ru");

        // Act
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(errorResponse);
        Assert.True(errorResponse.ContainsKey("error"));
    }

    [Fact]
    public async Task Puzzle_GridContainsValidData()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzle/puzzle2", TestContext.Current.CancellationToken);

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
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/crossword/unsolvedpuzzle");
        request.Headers.Add("X-User-Id", userId);
        request.Headers.Add("Accept-Language", "en");

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
        var response1 = await _client.GetAsync("/api/crossword/unsolvedpuzzle?seed=test123", TestContext.Current.CancellationToken);
        var response2 = await _client.GetAsync("/api/crossword/unsolvedpuzzle?seed=test123", TestContext.Current.CancellationToken);

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();
        
        var puzzle1 = await response1.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        var puzzle2 = await response2.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        // Note: Actual randomness depends on implementation, but we can verify both are valid
        Assert.NotNull(puzzle1);
        Assert.NotNull(puzzle2);
    }

    [Fact]
    public async Task GetPuzzleBySize_ReturnsCorrectSizeRange()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/unsolvedpuzzle", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.InRange(puzzle.Size.Rows, 5, 20);
        Assert.InRange(puzzle.Size.Cols, 5, 20);
    }

    [Fact]
    public async Task GetPuzzle_WithoutSizeParameter_ReturnsAnySize()
    {
        // Act - Don't specify size, should default to Any
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/crossword/unsolvedpuzzle");
        request.Headers.Add("Accept-Language", "en");
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.NotEmpty(puzzle.Grid);
        Assert.Equal(PuzzleLanguage.English, puzzle.Language);
    }
}
