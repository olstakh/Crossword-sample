using CrossWords.Services.Models;
using CrossWords.Services.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services;

internal class FilePuzzleRepository : IPuzzleRepositoryReader
{
    private readonly string _puzzlesFilePath;
    private readonly ILogger<FilePuzzleRepository> _logger;

    public FilePuzzleRepository(string puzzlesFilePath, ILogger<FilePuzzleRepository> logger)
    {
        _puzzlesFilePath = puzzlesFilePath;
        _logger = logger;
    }

    public IEnumerable<CrosswordPuzzle> LoadAllPuzzles()
    {
        try
        {
            // Load puzzles from JSON file
            var jsonContent = File.ReadAllText(_puzzlesFilePath);
            var puzzleList = JsonSerializer.Deserialize<List<CrosswordPuzzle>>(jsonContent);

            return puzzleList ?? Enumerable.Empty<CrosswordPuzzle>();
        }
        catch (Exception ex)
        {
            // Log the error and return empty collection
            _logger.LogError(ex, "Error loading puzzles from file: {FilePath}", _puzzlesFilePath);
            return Enumerable.Empty<CrosswordPuzzle>();
        }
    }

    public IEnumerable<CrosswordPuzzle> GetPuzzles(PuzzleSizeCategory sizeCategory = PuzzleSizeCategory.Any, PuzzleLanguage? language = null)
    {
        var allPuzzles = LoadAllPuzzles();

        // Filter by language if specified
        if (language.HasValue)
        {
            allPuzzles = allPuzzles.Where(p => p.Language == language.Value);
        }

        // Filter by size category if not Any
        if (sizeCategory != PuzzleSizeCategory.Any)
        {
            var (minSize, maxSize) = sizeCategory.GetSizeRange();
            allPuzzles = allPuzzles.Where(p => 
                p.Size.Rows >= minSize && p.Size.Rows <= maxSize &&
                p.Size.Cols >= minSize && p.Size.Cols <= maxSize);
        }

        return allPuzzles;
    }

    public CrosswordPuzzle? GetPuzzle(string puzzleId)
    {
        var allPuzzles = LoadAllPuzzles();
        return allPuzzles.FirstOrDefault(p => p.Id == puzzleId);
    }
}
