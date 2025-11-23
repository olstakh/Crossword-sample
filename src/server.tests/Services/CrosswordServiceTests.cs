using CrossWords.Models;
using CrossWords.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CrossWords.Tests.Services;

public class CrosswordServiceTests
{
    private readonly CrosswordService _service;
    private readonly Mock<IPuzzleRepository> _mockRepository;

    public CrosswordServiceTests()
    {
        // Create mock puzzles
        var mockPuzzles = new List<CrosswordPuzzle>
        {
            new CrosswordPuzzle
            {
                Id = "puzzle1",
                Title = "Easy Cryptogram",
                Language = PuzzleLanguage.English,
                Size = new PuzzleSize { Rows = 5, Cols = 5 },
                Grid = new List<List<string>>
                {
                    new List<string> { "C", "A", "T", "S", "#" },
                    new List<string> { "O", "#", "O", "#", "D" },
                    new List<string> { "D", "O", "G", "S", "#" },
                    new List<string> { "E", "#", "#", "#", "A" },
                    new List<string> { "#", "R", "A", "T", "S" }
                }
            },
            new CrosswordPuzzle
            {
                Id = "puzzle2",
                Title = "Medium Cryptogram",
                Language = PuzzleLanguage.English,
                Size = new PuzzleSize { Rows = 10, Cols = 10 },
                Grid = new List<List<string>>
                {
                    new List<string> { "B", "I", "R", "D", "S", "#", "C", "A", "T", "#" },
                    new List<string> { "E", "#", "A", "#", "U", "N", "#", "O", "#", "D" },
                    new List<string> { "A", "N", "T", "S", "#", "#", "D", "O", "G", "#" },
                    new List<string> { "R", "#", "#", "H", "E", "N", "#", "#", "#", "S" },
                    new List<string> { "S", "U", "N", "#", "#", "#", "F", "O", "X", "#" },
                    new List<string> { "#", "P", "I", "G", "S", "#", "#", "W", "#", "#" },
                    new List<string> { "M", "O", "U", "S", "E", "#", "B", "A", "T", "#" },
                    new List<string> { "#", "#", "C", "#", "#", "L", "I", "O", "N", "#" },
                    new List<string> { "F", "I", "S", "H", "#", "#", "#", "#", "#", "#" },
                    new List<string> { "#", "#", "#", "#", "B", "E", "A", "R", "#", "#" }
                }
            },
            new CrosswordPuzzle
            {
                Id = "puzzle3",
                Title = "Big Cryptogram Challenge",
                Language = PuzzleLanguage.English,
                Size = new PuzzleSize { Rows = 16, Cols = 16 },
                Grid = new List<List<string>>
                {
                    new List<string> { "T", "H", "E", "#", "Q", "U", "I", "C", "K", "#", "B", "R", "O", "W", "N", "#" },
                    new List<string> { "F", "O", "X", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" },
                    new List<string> { "J", "U", "M", "P", "S", "#", "O", "V", "E", "R", "#", "T", "H", "E", "#", "#" },
                    new List<string> { "L", "A", "Z", "Y", "#", "D", "O", "G", "#", "#", "#", "#", "#", "#", "#", "#" },
                    new List<string> { "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "A", "L", "L", "#", "#", "#" },
                    new List<string> { "Y", "O", "U", "#", "N", "E", "E", "D", "#", "I", "S", "#", "L", "O", "V", "E" },
                    new List<string> { "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" },
                    new List<string> { "T", "O", "#", "B", "E", "#", "O", "R", "#", "N", "O", "T", "#", "T", "O", "#" },
                    new List<string> { "B", "E", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" },
                    new List<string> { "#", "#", "T", "H", "A", "T", "#", "I", "S", "#", "T", "H", "E", "#", "#", "#" },
                    new List<string> { "Q", "U", "E", "S", "T", "I", "O", "N", "#", "#", "#", "#", "#", "#", "#", "#" },
                    new List<string> { "#", "#", "#", "#", "#", "#", "#", "#", "#", "M", "A", "Y", "#", "T", "H", "E" },
                    new List<string> { "F", "O", "R", "C", "E", "#", "B", "E", "#", "W", "I", "T", "H", "#", "Y", "O" },
                    new List<string> { "U", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" },
                    new List<string> { "L", "I", "F", "E", "#", "I", "S", "#", "G", "O", "O", "D", "#", "#", "#", "#" },
                    new List<string> { "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" }
                }
            }
        };

        // Setup mock repository
        _mockRepository = new Mock<IPuzzleRepository>(MockBehavior.Strict);
        _mockRepository.Setup(r => r.LoadAllPuzzles()).Returns(mockPuzzles);
        
        var serviceLogger = NullLogger<CrosswordService>.Instance;
        _service = new CrosswordService(_mockRepository.Object, serviceLogger);
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
