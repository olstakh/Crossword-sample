using CrossWords.Services.Abstractions;
using CrossWords.Services.Models;

namespace CrossWords.Services.Testing;

/// <summary>
/// In-memory implementation of puzzle repository for testing
/// Thread-safe for parallel test execution
/// </summary>
public class InMemoryPuzzleRepository : IPuzzleRepositoryReader, IPuzzleRepositoryPersister
{
    private readonly Dictionary<string, CrosswordPuzzle> _puzzles = new();
    private readonly object _lock = new();

    public InMemoryPuzzleRepository()
    {
    }

    public InMemoryPuzzleRepository(IEnumerable<CrosswordPuzzle> initialPuzzles)
    {
        foreach (var puzzle in initialPuzzles)
        {
            _puzzles[puzzle.Id] = puzzle;
        }
    }

    public IEnumerable<CrosswordPuzzle> LoadAllPuzzles()
    {
        lock (_lock)
        {
            return _puzzles.Values.ToList();
        }
    }

    public void AddPuzzle(CrosswordPuzzle puzzle)
    {
        lock (_lock)
        {
            _puzzles[puzzle.Id] = puzzle;
        }
    }

    public void DeletePuzzle(string puzzleId)
    {
        lock (_lock)
        {
            if (!_puzzles.Remove(puzzleId))
            {
                throw new KeyNotFoundException($"Puzzle with ID '{puzzleId}' not found.");
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _puzzles.Clear();
        }
    }
}
