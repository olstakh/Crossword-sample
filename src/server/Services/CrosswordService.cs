using CrossWords.Models;

namespace CrossWords.Services;

public interface ICrosswordService
{
    CrosswordPuzzle GetPuzzle(string id);
    CrosswordPuzzle GetPuzzleBySize(PuzzleSizeCategory size, string? seed = null);
    List<string> GetAvailablePuzzleIds();
}

public class CrosswordService : ICrosswordService
{
    private readonly Dictionary<string, CrosswordPuzzle> _cachedPuzzles;

    public CrosswordService()
    {
        _cachedPuzzles = InitializePuzzles();
    }

    public CrosswordPuzzle GetPuzzle(string id)
    {
        // Setting default puzzle here, since i need to change puzzle generator to be 2D
        return _cachedPuzzles.GetValueOrDefault(id) ?? _cachedPuzzles["puzzle1"];
    }

    public CrosswordPuzzle GetPuzzleBySize(PuzzleSizeCategory size, string? seed = null)
    {
        // Generate a deterministic puzzle based on size and seed
        var puzzleId = $"{size}_{seed ?? DateTime.UtcNow.ToString("yyyyMMdd")}";
        
        if (_cachedPuzzles.TryGetValue(puzzleId, out var cachedPuzzle))
        {
            return cachedPuzzle;
        }

        // Generate new puzzle based on size
        var puzzle = GeneratePuzzleBySize(size, puzzleId);
        _cachedPuzzles[puzzleId] = puzzle;
        return puzzle;
    }

    private CrosswordPuzzle GeneratePuzzleBySize(PuzzleSizeCategory size, string puzzleId)
    {
        var (minSize, maxSize) = size switch
        {
            PuzzleSizeCategory.Small => (5, 8),
            PuzzleSizeCategory.Medium => (9, 14),
            PuzzleSizeCategory.Big => (15, 20),
            _ => (5, 8) // default to small
        };

        // Use seed for deterministic random
        var random = new Random(puzzleId.GetHashCode());
        var gridSize = random.Next(minSize, maxSize + 1);

        // For now, return one of the hardcoded puzzles scaled to match requested size category
        // This is temporary until you implement the full 2D crossword generator
        return size switch
        {
            PuzzleSizeCategory.Small => _cachedPuzzles["puzzle1"],
            PuzzleSizeCategory.Medium => _cachedPuzzles["puzzle2"],
            PuzzleSizeCategory.Big => _cachedPuzzles["puzzle3"],
            _ => _cachedPuzzles["puzzle1"]
        };
    }

    public List<string> GetAvailablePuzzleIds()
    {
        // Return suggested puzzle IDs - any string can be used as an ID
        return new List<string> { "puzzle1", "puzzle2", "puzzle3" };
    }

    private Dictionary<string, CrosswordPuzzle> InitializePuzzles()
    {
        var puzzles = new Dictionary<string, CrosswordPuzzle>();

        // Puzzle 1 - Small puzzle
        puzzles["puzzle1"] = new CrosswordPuzzle
        {
            Id = "puzzle1",
            Title = "Easy Cryptogram",
            Size = new PuzzleSize { Rows = 5, Cols = 5 },
            Grid = new List<List<string>>
            {
                new() { "C", "A", "T", "S", "#" },
                new() { "O", "#", "O", "#", "D" },
                new() { "D", "O", "G", "S", "#" },
                new() { "E", "#", "#", "#", "A" },
                new() { "#", "R", "A", "T", "S" }
            }
        };

        // Puzzle 2 - Medium puzzle
        puzzles["puzzle2"] = new CrosswordPuzzle
        {
            Id = "puzzle2",
            Title = "Medium Cryptogram",
            Size = new PuzzleSize { Rows = 6, Cols = 6 },
            Grid = new List<List<string>>
            {
                new() { "B", "I", "R", "D", "S", "#" },
                new() { "E", "#", "A", "#", "U", "N" },
                new() { "A", "N", "T", "S", "#", "#" },
                new() { "R", "#", "#", "H", "E", "N" },
                new() { "S", "U", "N", "#", "#", "#" },
                new() { "#", "P", "I", "G", "S", "#" }
            }
        };

        // Puzzle 3 - Big puzzle (16x16)
        puzzles["puzzle3"] = new CrosswordPuzzle
        {
            Id = "puzzle3",
            Title = "Big Cryptogram Challenge",
            Size = new PuzzleSize { Rows = 16, Cols = 16 },
            Grid = new List<List<string>>
            {
                new() { "T", "H", "E", "#", "Q", "U", "I", "C", "K", "#", "B", "R", "O", "W", "N", "#" },
                new() { "F", "O", "X", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" },
                new() { "J", "U", "M", "P", "S", "#", "O", "V", "E", "R", "#", "T", "H", "E", "#", "#" },
                new() { "L", "A", "Z", "Y", "#", "D", "O", "G", "#", "#", "#", "#", "#", "#", "#", "#" },
                new() { "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "A", "L", "L", "#", "#", "#" },
                new() { "Y", "O", "U", "#", "N", "E", "E", "D", "#", "I", "S", "#", "L", "O", "V", "E" },
                new() { "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" },
                new() { "T", "O", "#", "B", "E", "#", "O", "R", "#", "N", "O", "T", "#", "T", "O", "#" },
                new() { "B", "E", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" },
                new() { "#", "#", "T", "H", "A", "T", "#", "I", "S", "#", "T", "H", "E", "#", "#", "#" },
                new() { "Q", "U", "E", "S", "T", "I", "O", "N", "#", "#", "#", "#", "#", "#", "#", "#" },
                new() { "#", "#", "#", "#", "#", "#", "#", "#", "#", "M", "A", "Y", "#", "T", "H", "E" },
                new() { "F", "O", "R", "C", "E", "#", "B", "E", "#", "W", "I", "T", "H", "#", "Y", "O" },
                new() { "U", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" },
                new() { "L", "I", "F", "E", "#", "I", "S", "#", "G", "O", "O", "D", "#", "#", "#", "#" },
                new() { "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#" }
            }
        };

        return puzzles;
    }
}
