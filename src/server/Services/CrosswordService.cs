using CrossWords.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services;

public interface ICrosswordService
{
    CrosswordPuzzle GetPuzzle(string id);
    CrosswordPuzzle GetPuzzleBySize(PuzzleSizeCategory size, string? seed = null);
    List<string> GetAvailablePuzzleIds();
}

public class CrosswordService : ICrosswordService
{
    private readonly Dictionary<string, CrosswordPuzzle> _cachedPuzzles;
    private readonly string _puzzlesFilePath;
    private readonly ILogger<CrosswordService> _logger;

    public CrosswordService(string puzzlesFilePath, ILogger<CrosswordService> logger)
    {
        _puzzlesFilePath = puzzlesFilePath;
        _logger = logger;
        _cachedPuzzles = InitializePuzzles();
    }

    public CrosswordPuzzle GetPuzzle(string id)
    {
        // Setting default puzzle here, since i need to change puzzle generator to be 2D
        return _cachedPuzzles.GetValueOrDefault(id) ?? _cachedPuzzles["puzzle1"];
    }

    public CrosswordPuzzle GetPuzzleBySize(PuzzleSizeCategory size, string? seed = null)
    {
        // Generate a deterministic puzzle based on size and seed
        var puzzleId = $"{size}_{seed ?? DateTime.UtcNow.ToString("yyyyMMdd")}";
        
        if (_cachedPuzzles.TryGetValue(puzzleId, out var cachedPuzzle))
        {
            return cachedPuzzle;
        }

        // Generate new puzzle based on size
        var puzzle = GeneratePuzzleBySize(size, puzzleId);
        _cachedPuzzles[puzzleId] = puzzle;
        return puzzle;
    }

    private CrosswordPuzzle GeneratePuzzleBySize(PuzzleSizeCategory size, string puzzleId)
    {
        var (minSize, maxSize) = size switch
        {
            PuzzleSizeCategory.Small => (5, 8),
            PuzzleSizeCategory.Medium => (9, 14),
            PuzzleSizeCategory.Big => (15, 20),
            _ => (5, 8) // default to small
        };

        // Use seed for deterministic random
        var random = new Random(puzzleId.GetHashCode());
        var gridSize = random.Next(minSize, maxSize + 1);

        // For now, return one of the hardcoded puzzles scaled to match requested size category
        // This is temporary until you implement the full 2D crossword generator
        return size switch
        {
            PuzzleSizeCategory.Small => _cachedPuzzles["puzzle1"],
            PuzzleSizeCategory.Medium => _cachedPuzzles["puzzle2"],
            PuzzleSizeCategory.Big => _cachedPuzzles["puzzle3"],
            _ => _cachedPuzzles["puzzle1"]
        };
    }

    public List<string> GetAvailablePuzzleIds()
    {
        // Return suggested puzzle IDs - any string can be used as an ID
        return new List<string> { "puzzle1", "puzzle2", "puzzle3" };
    }

    private Dictionary<string, CrosswordPuzzle> InitializePuzzles()
    {
        var puzzles = new Dictionary<string, CrosswordPuzzle>();

        try
        {
            // Load puzzles from JSON file
            var jsonContent = File.ReadAllText(_puzzlesFilePath);
            var puzzleList = JsonSerializer.Deserialize<List<CrosswordPuzzle>>(jsonContent);

            if (puzzleList != null)
            {
                foreach (var puzzle in puzzleList)
                {
                    puzzles[puzzle.Id] = puzzle;
                }
            }
        }
        catch (Exception ex)
        {
            // Log the error and fall back to empty dictionary
            _logger.LogError(ex, "Error loading puzzles from file: {FilePath}", _puzzlesFilePath);
        }

        return puzzles;
    }
}
