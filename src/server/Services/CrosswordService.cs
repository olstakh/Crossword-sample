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
            },
            Numbers = new List<List<int>>
            {
                new() { 1, 2, 3, 0, 4, 5, 6, 7, 8, 0, 9, 10, 11, 12, 13, 0 },
                new() { 14, 11, 15, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new() { 16, 5, 17, 18, 19, 0, 11, 20, 3, 10, 0, 1, 2, 3, 0, 0 },
                new() { 21, 22, 23, 24, 0, 25, 11, 26, 0, 0, 0, 0, 0, 0, 0, 0 },
                new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 22, 21, 21, 0, 0, 0 },
                new() { 24, 11, 5, 0, 13, 3, 3, 25, 0, 6, 19, 0, 21, 11, 20, 3 },
                new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new() { 1, 11, 0, 9, 3, 0, 11, 10, 0, 13, 11, 1, 0, 1, 11, 0 },
                new() { 9, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new() { 0, 0, 1, 2, 22, 1, 0, 6, 19, 0, 1, 2, 3, 0, 0, 0 },
                new() { 4, 5, 3, 19, 1, 6, 11, 13, 0, 0, 0, 0, 0, 0, 0, 0 },
                new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 17, 22, 24, 0, 1, 2, 3 },
                new() { 14, 11, 10, 7, 3, 0, 9, 3, 0, 12, 6, 1, 2, 0, 24, 11 },
                new() { 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new() { 21, 6, 14, 3, 0, 6, 19, 0, 26, 11, 11, 25, 0, 0, 0, 0 },
                new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            LetterMapping = new Dictionary<int, string>
            {
                { 1, "T" }, { 2, "H" }, { 3, "E" }, { 4, "Q" }, { 5, "U" },
                { 6, "I" }, { 7, "C" }, { 8, "K" }, { 9, "B" }, { 10, "R" },
                { 11, "O" }, { 12, "W" }, { 13, "N" }, { 14, "F" }, { 15, "X" },
                { 16, "J" }, { 17, "M" }, { 18, "P" }, { 19, "S" }, { 20, "V" },
                { 21, "L" }, { 22, "A" }, { 23, "Z" }, { 24, "Y" }, { 25, "D" },
                { 26, "G" }
            },
            InitiallyRevealed = new List<int> { 1, 3, 11 } // Reveal T, E, O at start
        };

        return puzzles;
    }
}
