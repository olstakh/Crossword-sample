using CrossWords.Services.Abstractions;

namespace CrossWords.Services.Testing;

/// <summary>
/// In-memory implementation of user progress repository for testing
/// Thread-safe for parallel test execution
/// </summary>
public class InMemoryUserProgressRepository : IUserProgressRepositoryReader, IUserProgressRepositoryWriter
{
    private readonly Dictionary<string, HashSet<string>> _userProgress = new();
    private readonly object _lock = new();

    public bool IsPuzzleSolved(string userId, string puzzleId)
    {
        lock (_lock)
        {
            return _userProgress.TryGetValue(userId, out var solvedPuzzles) 
                && solvedPuzzles.Contains(puzzleId);
        }
    }

    public void RecordSolvedPuzzle(string userId, string puzzleId)
    {
        lock (_lock)
        {
            if (!_userProgress.ContainsKey(userId))
            {
                _userProgress[userId] = new HashSet<string>();
            }
            _userProgress[userId].Add(puzzleId);
        }
    }

    public HashSet<string> GetSolvedPuzzles(string userId)
    {
        lock (_lock)
        {
            if (_userProgress.TryGetValue(userId, out var solvedPuzzles))
            {
                return new HashSet<string>(solvedPuzzles);
            }
            return new HashSet<string>();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _userProgress.Clear();
        }
    }
}
