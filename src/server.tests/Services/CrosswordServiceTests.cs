using CrossWords.Models;
using CrossWords.Services;

namespace CrossWords.Tests.Services;

public class CrosswordServiceTests
{
    private readonly CrosswordService _service;

    public CrosswordServiceTests()
    {
        // Use the puzzles.json file from the test project or server project
        var puzzlesFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "puzzles.json");
        _service = new CrosswordService(puzzlesFilePath);
    }

    [Fact]
    public void GetPuzzle_WithValidId_ReturnsPuzzle()
    {
        // Arrange
        var puzzleId = "puzzle1";

        // Act
        var result = _service.GetPuzzle(puzzleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(puzzleId, result.Id);
        Assert.Equal("Easy Cryptogram", result.Title);
        Assert.Equal(5, result.Size.Rows);
        Assert.Equal(5, result.Size.Cols);
        Assert.NotEmpty(result.Grid);
    }

    [Fact]
    public void GetPuzzle_WithInvalidId_ReturnsDefaultPuzzle()
    {
        // Arrange
        var puzzleId = "nonexistent";

        // Act
        var result = _service.GetPuzzle(puzzleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("puzzle1", result.Id); // Should return default puzzle1
    }

    [Theory]
    [InlineData(PuzzleSizeCategory.Small, "puzzle1")]
    [InlineData(PuzzleSizeCategory.Medium, "puzzle2")]
    [InlineData(PuzzleSizeCategory.Big, "puzzle3")]
    public void GetPuzzleBySize_ReturnsCorrectPuzzle(PuzzleSizeCategory size, string expectedPuzzleId)
    {
        // Arrange
        var seed = "test-seed";

        // Act
        var result = _service.GetPuzzleBySize(size, seed);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Grid);
        
        // Verify size is within expected range
        switch (size)
        {
            case PuzzleSizeCategory.Small:
                Assert.InRange(result.Size.Rows, 5, 8);
                Assert.InRange(result.Size.Cols, 5, 8);
                Assert.Equal(expectedPuzzleId, result.Id);
                break;
            case PuzzleSizeCategory.Medium:
                Assert.InRange(result.Size.Rows, 6, 14);
                Assert.InRange(result.Size.Cols, 6, 14);
                Assert.Equal(expectedPuzzleId, result.Id);
                break;
            case PuzzleSizeCategory.Big:
                Assert.InRange(result.Size.Rows, 15, 20);
                Assert.InRange(result.Size.Cols, 15, 20);
                Assert.Equal(expectedPuzzleId, result.Id);
                break;
        }
    }

    [Fact]
    public void GetPuzzleBySize_WithSameSeed_ReturnsCachedPuzzle()
    {
        // Arrange
        var size = PuzzleSizeCategory.Medium;
        var seed = "consistent-seed";

        // Act
        var result1 = _service.GetPuzzleBySize(size, seed);
        var result2 = _service.GetPuzzleBySize(size, seed);

        // Assert
        Assert.Same(result1, result2); // Should return same cached instance
    }

    [Fact]
    public void GetAvailablePuzzleIds_ReturnsAllPuzzles()
    {
        // Act
        var result = _service.GetAvailablePuzzleIds();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("puzzle1", result);
        Assert.Contains("puzzle2", result);
        Assert.Contains("puzzle3", result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Puzzle_Grid_ContainsOnlyValidCharacters()
    {
        // Arrange
        var puzzles = new[] { "puzzle1", "puzzle2", "puzzle3" };

        foreach (var puzzleId in puzzles)
        {
            // Act
            var puzzle = _service.GetPuzzle(puzzleId);

            // Assert
            foreach (var row in puzzle.Grid)
            {
                foreach (var cell in row)
                {
                    // Each cell should be either "#" or a single uppercase letter
                    Assert.True(
                        cell == "#" || (cell.Length == 1 && char.IsLetter(cell[0]) && char.IsUpper(cell[0])),
                        $"Invalid cell value: {cell} in puzzle {puzzleId}"
                    );
                }
            }
        }
    }

    [Fact]
    public void Puzzle_Grid_SizeMatchesMetadata()
    {
        // Arrange
        var puzzles = new[] { "puzzle1", "puzzle2", "puzzle3" };

        foreach (var puzzleId in puzzles)
        {
            // Act
            var puzzle = _service.GetPuzzle(puzzleId);

            // Assert
            Assert.Equal(puzzle.Size.Rows, puzzle.Grid.Count);
            foreach (var row in puzzle.Grid)
            {
                Assert.Equal(puzzle.Size.Cols, row.Count);
            }
        }
    }
}
