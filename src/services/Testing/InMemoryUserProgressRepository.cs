using CrossWords.Services.Abstractions;
using CrossWords.Services.Models;

namespace CrossWords.Services.Testing;

/// <summary>
/// In-memory implementation of user progress repository for testing
/// Thread-safe for parallel test execution
/// </summary>
public class InMemoryUserProgressRepository : IUserProgressRepositoryReader, IUserProgressRepositoryWriter
{
    private readonly Dictionary<string, Dictionary<PuzzleId, DateTime>> _userProgress = new();
    private readonly object _lock = new();

    public bool IsPuzzleSolved(string userId, PuzzleId puzzleId)
    {
        lock (_lock)
        {
            return _userProgress.TryGetValue(userId, out var solvedPuzzles) 
                && solvedPuzzles.ContainsKey(puzzleId);
        }
    }

    public void RecordSolvedPuzzle(string userId, PuzzleId puzzleId)
    {
        lock (_lock)
        {
            if (!_userProgress.ContainsKey(userId))
            {
                _userProgress[userId] = new Dictionary<PuzzleId, DateTime>();
            }
            _userProgress[userId][puzzleId] = DateTime.UtcNow;
        }
    }

    public void ForgetPuzzles(string userId, IEnumerable<PuzzleId> puzzleIds)
    {
        lock (_lock)
        {
            if (!_userProgress.TryGetValue(userId, out var solvedPuzzles))
            {
                return;
            }

            foreach (var puzzleId in puzzleIds)
            {
                solvedPuzzles.Remove(puzzleId);
            }
        }
    }

    public HashSet<PuzzleId> GetSolvedPuzzles(string userId)
    {
        lock (_lock)
        {
            if (_userProgress.TryGetValue(userId, out var solvedPuzzles))
            {
                return new HashSet<PuzzleId>(solvedPuzzles.Keys);
            }
            return new HashSet<PuzzleId>();
        }
    }

    public IEnumerable<string> GetAllUsers()
    {
        lock (_lock)
        {
            return _userProgress.Keys.ToList();
        }
    }

    public IEnumerable<UserProgressRecord> GetAllUserProgress()
    {
        lock (_lock)
        {
            var records = new List<UserProgressRecord>();
            foreach (var (userId, puzzles) in _userProgress)
            {
                foreach (var (puzzleId, solvedAt) in puzzles)
                {
                    records.Add(new UserProgressRecord
                    {
                        UserId = userId,
                        PuzzleId = puzzleId,
                        SolvedAt = solvedAt
                    });
                }
            }
            return records;
        }
    }

    public void ImportUserProgress(IEnumerable<UserProgressRecord> records)
    {
        lock (_lock)
        {
            _userProgress.Clear();
            
            foreach (var record in records)
            {
                if (!_userProgress.ContainsKey(record.UserId))
                {
                    _userProgress[record.UserId] = new Dictionary<PuzzleId, DateTime>();
                }
                _userProgress[record.UserId][record.PuzzleId] = record.SolvedAt;
            }
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
