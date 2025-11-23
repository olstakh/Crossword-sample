using CrossWords.Models;
using CrossWords.Exceptions;
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
        if (!_cachedPuzzles.TryGetValue(id, out var puzzle))
        {
            throw new PuzzleNotFoundException($"Puzzle with ID '{id}' was not found.");
        }
        
        return puzzle;
    }

    public CrosswordPuzzle GetPuzzle(PuzzleRequest request)
    {
        // Get puzzles filtered by language
        var languagePuzzles = _cachedPuzzles.Values.Where(p => p.Language == request.Language).ToList();
        
        if (!languagePuzzles.Any())
        {
            throw new PuzzleNotFoundException(
                $"No puzzles found for language '{request.Language}'. Please try a different language.");
        }

        // Filter by size category
        var (minSize, maxSize) = request.SizeCategory.GetSizeRange();

        var matchingPuzzles = languagePuzzles
            .Where(p => p.Size.Rows >= minSize && p.Size.Rows <= maxSize)
            .ToList();

        if (!matchingPuzzles.Any())
        {
            throw new PuzzleNotFoundException(
                $"No puzzles found for language '{request.Language}' and size '{request.SizeCategory}'. Please try a different combination.");
        }

        // Use seed for deterministic selection
        var random = 
            request.Seed == null
            ? new Random()
            : new Random(request.Seed.GetHashCode());
        var selectedPuzzle = matchingPuzzles[random.Next(matchingPuzzles.Count)];
        
        return selectedPuzzle;
    }

    public List<string> GetAvailablePuzzleIds(PuzzleLanguage? language = null)
    {
        return _cachedPuzzles.Values
            .Where(p => language.HasValue ? p.Language == language.Value : true)
            .Select(p => p.Id)
            .ToList();
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
