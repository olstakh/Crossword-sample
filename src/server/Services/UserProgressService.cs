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
    // In-memory storage - replace with database for production
    private readonly Dictionary<string, HashSet<string>> _userSolvedPuzzles = new();
    private readonly object _lock = new();

    public UserProgressService(ICrosswordService crosswordService)
    {
        _crosswordService = crosswordService;
    }

    public UserProgress GetUserProgress(string userId)
    {
        lock (_lock)
        {
            if (!_userSolvedPuzzles.ContainsKey(userId))
            {
                return new UserProgress
                {
                    UserId = userId,
                    SolvedPuzzleIds = new List<string>(),
                    TotalPuzzlesSolved = 0,
                    LastPlayed = DateTime.UtcNow
                };
            }

            var solvedIds = _userSolvedPuzzles[userId].ToList();
            return new UserProgress
            {
                UserId = userId,
                SolvedPuzzleIds = solvedIds,
                TotalPuzzlesSolved = solvedIds.Count,
                LastPlayed = DateTime.UtcNow
            };
        }
    }

    public void RecordSolvedPuzzle(string userId, string puzzleId)
    {
        lock (_lock)
        {
            if (!_userSolvedPuzzles.ContainsKey(userId))
            {
                _userSolvedPuzzles[userId] = new HashSet<string>();
            }

            _userSolvedPuzzles[userId].Add(puzzleId);
        }
    }

    public AvailablePuzzlesResponse GetAvailablePuzzles(string userId, PuzzleLanguage? language = null)
    {
        var allPuzzles = _crosswordService.GetAvailablePuzzleIds(language);
        
        lock (_lock)
        {
            var solvedPuzzles = _userSolvedPuzzles.ContainsKey(userId) 
                ? _userSolvedPuzzles[userId].ToList() 
                : new List<string>();

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
    }

    public bool HasSolvedPuzzle(string userId, string puzzleId)
    {
        lock (_lock)
        {
            return _userSolvedPuzzles.ContainsKey(userId) 
                && _userSolvedPuzzles[userId].Contains(puzzleId);
        }
    }
}
