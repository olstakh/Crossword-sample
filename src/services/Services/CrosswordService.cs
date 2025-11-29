using CrossWords.Services.Models;
using CrossWords.Services.Exceptions;
using CrossWords.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services;

internal class CrosswordService : ICrosswordService
{
    private readonly Lazy<IReadOnlyDictionary<string, CrosswordPuzzle>> _cachedPuzzles;
    private readonly ILogger<CrosswordService> _logger;
    private readonly IUserProgressRepositoryReader _userProgressRepository;

    private IReadOnlyDictionary<string, CrosswordPuzzle> CachedPuzzles => _cachedPuzzles.Value;

    public CrosswordService(
        IPuzzleRepositoryReader puzzleRepository,
        IUserProgressRepositoryReader userProgressRepository, 
        ILogger<CrosswordService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userProgressRepository = userProgressRepository ?? throw new ArgumentNullException(nameof(userProgressRepository));
        
        _cachedPuzzles = new(() => InitializePuzzles(puzzleRepository), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public CrosswordPuzzle GetPuzzle(string id)
    {
        if (CachedPuzzles.TryGetValue(id, out var puzzle))
        {
            return puzzle;
        }
        throw new PuzzleNotFoundException($"Puzzle with ID '{id}' was not found.");
    }

    public IEnumerable<CrosswordPuzzle> GetPuzzles(PuzzleRequest request)
    {
        var (minSize, maxSize) = request.SizeCategory.GetSizeRange();

        var solvedPuzzleIds =
            request.UserId != null
                ? _userProgressRepository.GetSolvedPuzzles(request.UserId)
                : new HashSet<string>();

        return CachedPuzzles.Values
            .Where(p => p.Language == request.Language)
            .Where(p => p.Size.Rows >= minSize && p.Size.Rows <= maxSize)
            .Where(p => !solvedPuzzleIds.Contains(p.Id));
    }

    public IReadOnlyList<string> GetAvailablePuzzleIds(PuzzleLanguage? language = null)
    {
        return CachedPuzzles.Values
            .Where(p => language.HasValue ? p.Language == language.Value : true)
            .Select(p => p.Id)
            .ToList();
    }

    private Dictionary<string, CrosswordPuzzle> InitializePuzzles(IPuzzleRepositoryReader puzzleRepository)
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
