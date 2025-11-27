namespace CrossWords.Services;

using CrossWords.Models;

/// <summary>
/// Service for managing user progress and solved puzzles
/// Uses in-memory storage (can be replaced with database later)
/// </summary>
public interface IUserProgressService
{
    /// <summary>
    /// Get user's progress including solved puzzles
    /// </summary>
    UserProgress GetUserProgress(string userId);
    
    /// <summary>
    /// Record that a user has solved a puzzle
    /// </summary>
    void RecordSolvedPuzzle(string userId, string puzzleId);
    
    /// <summary>
    /// Get available puzzles for user (excluding already solved)
    /// </summary>
    AvailablePuzzlesResponse GetAvailablePuzzles(string userId, PuzzleLanguage? language = null);
    
    /// <summary>
    /// Check if user has solved a specific puzzle
    /// </summary>
    bool HasSolvedPuzzle(string userId, string puzzleId);
}

public class UserProgressService : IUserProgressService
{
    private readonly ICrosswordService _crosswordService;
    private readonly IUserProgressRepository _repository;

    public UserProgressService(ICrosswordService crosswordService, IUserProgressRepository repository)
    {
        _crosswordService = crosswordService;
        _repository = repository;
    }

    public UserProgress GetUserProgress(string userId)
    {
        var solvedIds = _repository.GetSolvedPuzzles(userId).ToList();
        var availablePuzzleIds = _crosswordService.GetAvailablePuzzleIds();
        return new UserProgress
        {
            UserId = userId,
            SolvedPuzzleIds = solvedIds.Intersect(availablePuzzleIds).ToList(),
            TotalPuzzlesSolved = solvedIds.Count,
            LastPlayed = DateTime.UtcNow
        };
    }

    public void RecordSolvedPuzzle(string userId, string puzzleId)
    {
        _repository.RecordSolvedPuzzle(userId, puzzleId);
    }

    public AvailablePuzzlesResponse GetAvailablePuzzles(string userId, PuzzleLanguage? language = null)
    {
        var allPuzzles = _crosswordService.GetAvailablePuzzleIds(language);
        var solvedPuzzles = _repository.GetSolvedPuzzles(userId).ToList();

        var unsolvedPuzzles = allPuzzles
            .Where(id => !solvedPuzzles.Contains(id))
            .ToList();

        return new AvailablePuzzlesResponse
        {
            UnsolvedPuzzleIds = unsolvedPuzzles,
            SolvedPuzzleIds = solvedPuzzles.Where(id => allPuzzles.Contains(id)).ToList(),
            TotalAvailable = allPuzzles.Count,
            TotalSolved = solvedPuzzles.Count(id => allPuzzles.Contains(id))
        };
    }

    public bool HasSolvedPuzzle(string userId, string puzzleId)
    {
        return _repository.IsPuzzleSolved(userId, puzzleId);
    }
}
