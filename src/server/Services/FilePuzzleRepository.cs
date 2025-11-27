using CrossWords.Models;
using CrossWords.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services;

public class FilePuzzleRepository : IPuzzleRepository
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
}
