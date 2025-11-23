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
    public async Task GetDefaultPuzzle_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzle", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.NotEmpty(puzzle.Grid);
    }

    [Theory]
    [InlineData("Small")]
    [InlineData("Medium")]
    [InlineData("Big")]
    [InlineData("small")]
    [InlineData("medium")]
    [InlineData("big")]
    public async Task GetPuzzleBySize_WithValidSize_ReturnsSuccess(string size)
    {
        // Act
        var response = await _client.GetAsync($"/api/crossword/puzzle/size/{size}", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.NotEmpty(puzzle.Grid);
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

    [Fact]
    public async Task GetPuzzleBySize_Small_ReturnsSmallSizedGrid()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzle/size/small", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.InRange(puzzle.Size.Rows, 5, 8);
        Assert.InRange(puzzle.Size.Cols, 5, 8);
    }

    [Fact]
    public async Task GetPuzzleBySize_Medium_ReturnsMediumSizedGrid()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzle/size/medium", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.InRange(puzzle.Size.Rows, 6, 14);
        Assert.InRange(puzzle.Size.Cols, 6, 14);
    }

    [Fact]
    public async Task GetPuzzleBySize_Big_ReturnsBigSizedGrid()
    {
        // Act
        var response = await _client.GetAsync("/api/crossword/puzzle/size/big", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var puzzle = await response.Content.ReadFromJsonAsync<CrosswordPuzzle>(cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.NotNull(puzzle);
        Assert.InRange(puzzle.Size.Rows, 15, 20);
        Assert.InRange(puzzle.Size.Cols, 15, 20);
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
}
