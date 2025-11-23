using CrossWords.Models;
using CrossWords.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CrossWords.Tests.Services;

public class CrosswordServiceTests
{
    private readonly CrosswordService _service;

    public CrosswordServiceTests()
    {
        // Use the puzzles.json file from the test project or server project
        var puzzlesFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "puzzles.json");
        var repositoryLogger = NullLogger<FilePuzzleRepository>.Instance;
        var repository = new FilePuzzleRepository(puzzlesFilePath, repositoryLogger);
        
        var serviceLogger = NullLogger<CrosswordService>.Instance;
        _service = new CrosswordService(repository, serviceLogger);
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
    [InlineData(PuzzleSizeCategory.Small)]
    [InlineData(PuzzleSizeCategory.Medium)]
    [InlineData(PuzzleSizeCategory.Big)]
    public void GetPuzzle_WithPuzzleRequest_ReturnsCorrectSizedPuzzle(PuzzleSizeCategory size)
    {
        // Arrange
        var request = new PuzzleRequest
        {
            SizeCategory = size,
            Language = PuzzleLanguage.English,
            Seed = "test-seed"
        };

        // Act
        var result = _service.GetPuzzle(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Grid);
        
        // Verify size is within expected range
        var (minSize, maxSize) = size.GetSizeRange();
        Assert.InRange(result.Size.Rows, minSize, maxSize);
        Assert.InRange(result.Size.Cols, minSize, maxSize);
    }

    [Fact]
    public void GetPuzzle_WithPuzzleRequest_UsesDefaultValues()
    {
        // Arrange
        var request = new PuzzleRequest(); // Uses defaults: Medium, English

        // Act
        var result = _service.GetPuzzle(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Grid);
        Assert.Equal(PuzzleLanguage.English, result.Language);
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
        Assert.True(result.Count >= 3);
    }

    [Fact]
    public void GetAvailablePuzzleIds_WithLanguageFilter_ReturnsFilteredPuzzles()
    {
        // Act
        var result = _service.GetAvailablePuzzleIds(PuzzleLanguage.English);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
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
