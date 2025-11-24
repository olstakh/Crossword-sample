using CrossWords.Models;
using CrossWords.Exceptions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services;

public interface ICrosswordService
{
    CrosswordPuzzle GetPuzzle(string id);
    CrosswordPuzzle GetPuzzle(PuzzleRequest request);
    IReadOnlyList<string> GetAvailablePuzzleIds(PuzzleLanguage? language = null);
}

public class CrosswordService : ICrosswordService
{
    private readonly IReadOnlyDictionary<string, CrosswordPuzzle> _cachedPuzzles;
    private readonly ILogger<CrosswordService> _logger;
    private readonly IUserProgressRepository _userProgressRepository;

    public CrosswordService(
        IPuzzleRepository puzzleRepository,
        IUserProgressRepository userProgressRepository, 
        ILogger<CrosswordService> logger)
    {
        _logger = logger;
        _userProgressRepository = userProgressRepository;
        _cachedPuzzles = InitializePuzzles(puzzleRepository);
    }

    public CrosswordPuzzle GetPuzzle(string id)
    {
        if (_cachedPuzzles.TryGetValue(id, out var puzzle))
        {
            return puzzle;
        }
        throw new PuzzleNotFoundException($"Puzzle with ID '{id}' was not found.");
    }

    public CrosswordPuzzle GetPuzzle(PuzzleRequest request)
    {
        // Get puzzles filtered by language
        var languagePuzzles = _cachedPuzzles.Values.Where(p => p.Language == request.Language).ToList();
        
        // Filter by size category
        var (minSize, maxSize) = request.SizeCategory.GetSizeRange();

        var matchingPuzzles = languagePuzzles
            .Where(p => p.Size.Rows >= minSize && p.Size.Rows <= maxSize)
            .ToList();

        if (matchingPuzzles.Count == 0)
        {
            throw new PuzzleNotFoundException(
                $"No puzzles found for language '{request.Language}' and size '{request.SizeCategory}'. Please try a different combination.");
        }

        // If userId provided, filter out solved puzzles
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            var solvedPuzzleIds = _userProgressRepository.GetSolvedPuzzles(request.UserId);
            var unsolvedPuzzles = matchingPuzzles
                .Where(p => !solvedPuzzleIds.Contains(p.Id))
                .ToList();

            if (unsolvedPuzzles.Count == 0)
            {
                throw new PuzzleNotFoundException(
                    $"Congratulations! You've solved all {matchingPuzzles.Count} puzzles in '{request.Language}' for size '{request.SizeCategory}'. Try a different language or size category!");
            }

            matchingPuzzles = unsolvedPuzzles;
        }

        var selectedPuzzle = matchingPuzzles[Random.Shared.Next(matchingPuzzles.Count)];
        
        return selectedPuzzle;
    }

    public IReadOnlyList<string> GetAvailablePuzzleIds(PuzzleLanguage? language = null)
    {
        return _cachedPuzzles.Values
            .Where(p => language.HasValue ? p.Language == language.Value : true)
            .Select(p => p.Id)
            .ToList();
    }

    private Dictionary<string, CrosswordPuzzle> InitializePuzzles(IPuzzleRepository puzzleRepository)
    {
        try
        {
            return puzzleRepository.LoadAllPuzzles()
                .ToDictionary(p => p.Id, p => p);
        }
        catch (Exception ex)
        {
            // Log the error and fall back to empty dictionary
            _logger.LogError(ex, "Error initializing puzzles");
        }

        return new Dictionary<string, CrosswordPuzzle>();
    }
}
