using CrossWords.Models;

namespace CrossWords.Services;

public interface ICrosswordService
{
    CrosswordPuzzle GetPuzzle(string id);
    CrosswordPuzzle GetPuzzleBySize(string size, string? seed = null);
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

    public CrosswordPuzzle GetPuzzleBySize(string size, string? seed = null)
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

    private CrosswordPuzzle GeneratePuzzleBySize(string size, string puzzleId)
    {
        var (minSize, maxSize) = size.ToLower() switch
        {
            "small" => (5, 8),
            "medium" => (9, 14),
            "big" => (15, 20),
            _ => (5, 8) // default to small
        };

        // Use seed for deterministic random
        var random = new Random(puzzleId.GetHashCode());
        var gridSize = random.Next(minSize, maxSize + 1);

        // For now, return one of the hardcoded puzzles scaled to match requested size category
        // This is temporary until you implement the full 2D crossword generator
        if (size.ToLower() == "small")
        {
            return _cachedPuzzles["puzzle1"];
        }
        else if (size.ToLower() == "medium")
        {
            return _cachedPuzzles["puzzle2"];
        }
        else
        {
            // For big, scale up puzzle2
            return _cachedPuzzles["puzzle2"];
        }
    }

    public List<string> GetAvailablePuzzleIds()
    {
        // Return suggested puzzle IDs - any string can be used as an ID
        return new List<string> { "puzzle1", "puzzle2" };
    }

    private Dictionary<string, CrosswordPuzzle> InitializePuzzles()
    {
        var puzzles = new Dictionary<string, CrosswordPuzzle>();

        // Puzzle 1 - Cryptogram style
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
            },
            Numbers = new List<List<int>>
            {
                new() { 1, 2, 3, 4, 0 },
                new() { 5, 0, 5, 0, 6 },
                new() { 6, 5, 7, 4, 0 },
                new() { 8, 0, 0, 0, 2 },
                new() { 0, 9, 2, 3, 4 }
            },
            LetterMapping = new Dictionary<int, string>
            {
                { 1, "C" }, { 2, "A" }, { 3, "T" }, { 4, "S" },
                { 5, "O" }, { 6, "D" }, { 7, "G" }, { 8, "E" }, { 9, "R" }
            },
            InitiallyRevealed = new List<int> { 2, 5 } // Reveal A and O at start
        };

        // Puzzle 2 - Medium difficulty
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
            },
            Numbers = new List<List<int>>
            {
                new() { 1, 2, 3, 4, 5, 0 },
                new() { 6, 0, 7, 0, 8, 9 },
                new() { 7, 9, 10, 5, 0, 0 },
                new() { 3, 0, 0, 11, 6, 9 },
                new() { 5, 8, 9, 0, 0, 0 },
                new() { 0, 12, 2, 13, 5, 0 }
            },
            LetterMapping = new Dictionary<int, string>
            {
                { 1, "B" }, { 2, "I" }, { 3, "R" }, { 4, "D" }, { 5, "S" },
                { 6, "E" }, { 7, "A" }, { 8, "U" }, { 9, "N" }, { 10, "T" },
                { 11, "H" }, { 12, "P" }, { 13, "G" }
            },
            InitiallyRevealed = new List<int> { 5, 9 } // Reveal S and N at start
        };

        return puzzles;
    }
}
