using CrossWords.Services.Models;
using CrossWords.Services;
using CrossWords.Services.Abstractions;
using CrossWords.Services.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Neovolve.Logging.Xunit;

namespace CrossWords.Services.Tests;

public class CrosswordServiceTests
{
    private readonly CrosswordService _service;
    private readonly Mock<IPuzzleRepositoryReader> _mockPuzzleRepository = new(MockBehavior.Strict);
    private readonly Mock<IUserProgressRepositoryReader> _mockUserRepository = new(MockBehavior.Strict);

    public CrosswordServiceTests(ITestOutputHelper output)
    {
        _service = new CrosswordService(_mockPuzzleRepository.Object, _mockUserRepository.Object, output.BuildLoggerFor<CrosswordService>());
    }

    [Fact]
    public void GetPuzzle_WithValidId_ReturnsPuzzle()
    {
        // Arrange
        var puzzle = GenerateMockPuzzle("puzzle1");
        _mockPuzzleRepository.Setup(r => r.LoadAllPuzzles()).Returns(new[] { puzzle }).Verifiable();

        // Act
        var result = _service.GetPuzzle("puzzle1");
        _ = _service.GetPuzzle("puzzle1"); // Call again to test caching

        // Assert
        Assert.Equal(puzzle, result);
        _mockPuzzleRepository.Verify(r => r.LoadAllPuzzles(), Times.Once); // Ensure LoadAllPuzzles called only once due to caching
    }

    [Fact]
    public void GetPuzzle_WithInvalidId_ThrowsException()
    {        
        // Arrange
        _mockPuzzleRepository.Setup(r => r.LoadAllPuzzles()).Returns([]);
        var puzzleId = "nonexistent";

        // Act & Assert
        var exception = Assert.Throws<PuzzleNotFoundException>(() => _service.GetPuzzle(puzzleId));
        Assert.Contains("nonexistent", exception.Message);
    }

    [Theory]
    [InlineData(PuzzleSizeCategory.Small)]
    [InlineData(PuzzleSizeCategory.Medium)]
    [InlineData(PuzzleSizeCategory.Big)]
    public void GetPuzzles_WithPuzzleRequest_ReturnsCorrectSizedPuzzle(PuzzleSizeCategory size)
    {
        _mockPuzzleRepository.Setup(r => r.LoadAllPuzzles()).Returns(new[]
        {
            GenerateMockPuzzle(PuzzleSizeCategory.Small.ToString(), PuzzleLanguage.English, PuzzleSizeCategory.Small),
            GenerateMockPuzzle(PuzzleSizeCategory.Medium.ToString(), PuzzleLanguage.English, PuzzleSizeCategory.Medium),
            GenerateMockPuzzle(PuzzleSizeCategory.Big.ToString(), PuzzleLanguage.English, PuzzleSizeCategory.Big),
        });

        // Arrange
        var request = new PuzzleRequest
        {
            SizeCategory = size,
            Language = PuzzleLanguage.English,
        };

        // Act
        var result = _service.GetPuzzles(request);

        // Assert
        var puzzle = Assert.Single(result);

        // Verify size is within expected range
        var (minSize, maxSize) = size.GetSizeRange();
        Assert.InRange(puzzle.Size.Rows, minSize, maxSize);
        Assert.InRange(puzzle.Size.Cols, minSize, maxSize);
        Assert.Equal(size.ToString(), puzzle.Id);
    }

    [Theory]
    [InlineData(PuzzleLanguage.English)]
    [InlineData(PuzzleLanguage.Russian)]
    [InlineData(PuzzleLanguage.Ukrainian)]
    public void GetPuzzles_WithPuzzleRequest_ReturnsCorrectLanguagePuzzle(PuzzleLanguage language)
    {
        _mockPuzzleRepository.Setup(r => r.LoadAllPuzzles()).Returns(new[]
        {
            GenerateMockPuzzle(PuzzleLanguage.English.ToString(), PuzzleLanguage.English, PuzzleSizeCategory.Any),
            GenerateMockPuzzle(PuzzleLanguage.Russian.ToString(), PuzzleLanguage.Russian, PuzzleSizeCategory.Any),
            GenerateMockPuzzle(PuzzleLanguage.Ukrainian.ToString(), PuzzleLanguage.Ukrainian, PuzzleSizeCategory.Any),
        });

        // Arrange
        var request = new PuzzleRequest
        {
            SizeCategory = PuzzleSizeCategory.Any,
            Language = language,
        };

        // Act
        var result = _service.GetPuzzles(request);

        // Assert
        var puzzle = Assert.Single(result);

        // Verify size is within expected range
        Assert.Equal(language.ToString(), puzzle.Id);
        Assert.Equal(language, puzzle.Language);
    }

    [Fact]
    public void GetPuzzles_WithPuzzleRequest_ReturnsUserSpecificPuzzles()
    {
        // Arrange
        var puzzle1 = GenerateMockPuzzle("puzzle1");
        var puzzle2 = GenerateMockPuzzle("puzzle2");
        _mockPuzzleRepository.Setup(r => r.LoadAllPuzzles()).Returns(new[] { puzzle1, puzzle2 });

        var solvedPuzzles = new HashSet<string> { "puzzle1" };
        _mockUserRepository.Setup(u => u.GetSolvedPuzzles("user123")).Returns(solvedPuzzles);

        var request = new PuzzleRequest
        {
            SizeCategory = PuzzleSizeCategory.Any,
            Language = PuzzleLanguage.English,
            UserId = "user123"
        };

        // Act
        var result = _service.GetPuzzles(request);

        // Assert
        var puzzle = Assert.Single(result);
        Assert.Equal(puzzle2, puzzle);
    }

    [Fact]
    public void GetAvailablePuzzleIds_ReturnsAllPuzzles()
    {
        _mockPuzzleRepository.Setup(r => r.LoadAllPuzzles()).Returns(new[]
        {
            GenerateMockPuzzle("puzzle1", PuzzleLanguage.English),
            GenerateMockPuzzle("puzzle2", PuzzleLanguage.Russian),
            GenerateMockPuzzle("puzzle3", PuzzleLanguage.English),
        });

        // Act
        var result = _service.GetAvailablePuzzleIds();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new[] { "puzzle1", "puzzle2", "puzzle3" }, result.Order());
    }

    [Fact]
    public void GetAvailablePuzzleIds_ReturnsLanguageSpecificPuzzles()
    {
        _mockPuzzleRepository.Setup(r => r.LoadAllPuzzles()).Returns(new[]
        {
            GenerateMockPuzzle("puzzle1", PuzzleLanguage.English),
            GenerateMockPuzzle("puzzle2", PuzzleLanguage.Russian),
            GenerateMockPuzzle("puzzle3", PuzzleLanguage.English),
        });

        // Act
        var result = _service.GetAvailablePuzzleIds(PuzzleLanguage.English);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new[] { "puzzle1", "puzzle3" }, result.Order());
    }    

    private CrosswordPuzzle GenerateMockPuzzle(string id, PuzzleLanguage language = PuzzleLanguage.English, PuzzleSizeCategory sizeCategory = PuzzleSizeCategory.Any)
    {
        var (rows, cols) = sizeCategory.GetSizeRange();
        return new CrosswordPuzzle
        {
            Id = id,
            Title = $"Mock Puzzle {id}",
            Language = language,
            Size = new PuzzleSize { Rows = rows, Cols = cols },
            Grid = Enumerable.Range(0, rows)
                .Select(_ => Enumerable.Range(0, cols).Select(__ => "A").ToList())
                .ToList()
        };
    }
}
