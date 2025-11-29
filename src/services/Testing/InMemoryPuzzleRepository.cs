using CrossWords.Services.Abstractions;
using CrossWords.Services.Models;

namespace CrossWords.Services.Testing;

/// <summary>
/// In-memory implementation of puzzle repository for testing
/// Thread-safe for parallel test execution
/// </summary>
public class InMemoryPuzzleRepository : IPuzzleRepositoryReader, IPuzzleRepositoryWriter
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

    public IEnumerable<CrosswordPuzzle> GetPuzzles(PuzzleSizeCategory sizeCategory = PuzzleSizeCategory.Any, PuzzleLanguage? language = null)
    {
        lock (_lock)
        {
            var puzzles = _puzzles.Values.AsEnumerable();

            // Filter by language if specified
            if (language.HasValue)
            {
                puzzles = puzzles.Where(p => p.Language == language.Value);
            }

            // Filter by size category if not Any
            if (sizeCategory != PuzzleSizeCategory.Any)
            {
                var (minSize, maxSize) = sizeCategory.GetSizeRange();
                puzzles = puzzles.Where(p => 
                    p.Size.Rows >= minSize && p.Size.Rows <= maxSize &&
                    p.Size.Cols >= minSize && p.Size.Cols <= maxSize);
            }

            return puzzles.ToList();
        }
    }

    public CrosswordPuzzle? GetPuzzle(string puzzleId)
    {
        lock (_lock)
        {
            return _puzzles.TryGetValue(puzzleId, out var puzzle) ? puzzle : null;
        }
    }

    public void AddPuzzle(CrosswordPuzzle puzzle)
    {
        lock (_lock)
        {
            _puzzles[puzzle.Id] = puzzle;
        }
    }

    public void AddPuzzles(IEnumerable<CrosswordPuzzle> puzzles)
    {
        lock (_lock)
        {
            foreach (var puzzle in puzzles)
            {
                _puzzles[puzzle.Id] = puzzle;
            }
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
