using CrossWords.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services;

public interface ICrosswordService
{
    CrosswordPuzzle GetPuzzle(string id);
    CrosswordPuzzle GetPuzzle(PuzzleRequest request);
    List<string> GetAvailablePuzzleIds(PuzzleLanguage? language = null);
}

public class CrosswordService : ICrosswordService
{
    private readonly Dictionary<string, CrosswordPuzzle> _cachedPuzzles;
    private readonly ILogger<CrosswordService> _logger;

    public CrosswordService(IPuzzleRepository puzzleRepository, ILogger<CrosswordService> logger)
    {
        _logger = logger;
        _cachedPuzzles = InitializePuzzles(puzzleRepository);
    }

    public CrosswordPuzzle GetPuzzle(string id)
    {
        // Setting default puzzle here, since i need to change puzzle generator to be 2D
        return _cachedPuzzles.GetValueOrDefault(id) ?? _cachedPuzzles["puzzle1"];
    }

    public CrosswordPuzzle GetPuzzle(PuzzleRequest request)
    {
        // Get puzzles filtered by language
        var languagePuzzles = _cachedPuzzles.Values.Where(p => p.Language == request.Language).ToList();
        
        if (!languagePuzzles.Any())
        {
            // Fallback to English if no puzzles in requested language
            languagePuzzles = _cachedPuzzles.Values.Where(p => p.Language == PuzzleLanguage.English).ToList();
        }

        // Filter by size category
        var (minSize, maxSize) = request.SizeCategory.GetSizeRange();

        var matchingPuzzles = languagePuzzles
            .Where(p => p.Size.Rows >= minSize && p.Size.Rows <= maxSize)
            .ToList();

        if (!matchingPuzzles.Any())
        {
            // Final fallback to first available puzzle
            return languagePuzzles.First();    
        }

        // Use seed for deterministic selection
        var random = new Random((request.Seed ?? DateTime.UtcNow.ToString("yyyyMMdd")).GetHashCode());
        var selectedPuzzle = matchingPuzzles[random.Next(matchingPuzzles.Count)];
        
        return selectedPuzzle;
    }

    public List<string> GetAvailablePuzzleIds(PuzzleLanguage? language = null)
    {
        if (language.HasValue)
        {
            return _cachedPuzzles.Values
                .Where(p => p.Language == language.Value)
                .Select(p => p.Id)
                .ToList();
        }
        
        return _cachedPuzzles.Keys.ToList();
    }

    private Dictionary<string, CrosswordPuzzle> InitializePuzzles(IPuzzleRepository puzzleRepository)
    {
        var puzzles = new Dictionary<string, CrosswordPuzzle>();

        try
        {
            var puzzleList = puzzleRepository.LoadAllPuzzles();

            foreach (var puzzle in puzzleList)
            {
                puzzles[puzzle.Id] = puzzle;
            }
        }
        catch (Exception ex)
        {
            // Log the error and fall back to empty dictionary
            _logger.LogError(ex, "Error initializing puzzles");
        }

        return puzzles;
    }
}
