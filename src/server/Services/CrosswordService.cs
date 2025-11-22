using CrossWords.Models;

namespace CrossWords.Services;

public interface ICrosswordService
{
    CrosswordPuzzle GetPuzzle(string id);
    List<string> GetAvailablePuzzleIds();
}

public class CrosswordService : ICrosswordService
{
    private readonly Dictionary<string, CrosswordPuzzle> _puzzles;

    public CrosswordService()
    {
        _puzzles = InitializePuzzles();
    }

    public CrosswordPuzzle GetPuzzle(string id)
    {
        return _puzzles.GetValueOrDefault(id) ?? _puzzles["puzzle1"];
    }

    public List<string> GetAvailablePuzzleIds()
    {
        return _puzzles.Keys.ToList();
    }

    private Dictionary<string, CrosswordPuzzle> InitializePuzzles()
    {
        var puzzles = new Dictionary<string, CrosswordPuzzle>();

        // Puzzle 1 - Same as the original
        puzzles["puzzle1"] = new CrosswordPuzzle
        {
            Id = "puzzle1",
            Title = "Easy Crossword",
            Size = new PuzzleSize { Rows = 10, Cols = 10 },
            Grid = new List<List<string>>
            {
                new() { "C", "A", "T", "#", "D", "O", "G", "#", "#", "#" },
                new() { "O", "#", "R", "#", "A", "#", "O", "#", "#", "#" },
                new() { "D", "R", "E", "A", "M", "#", "A", "L", "S", "O" },
                new() { "E", "#", "E", "#", "#", "#", "T", "#", "#", "#" },
                new() { "#", "#", "#", "P", "L", "A", "Y", "#", "#", "#" },
                new() { "#", "S", "U", "N", "#", "#", "#", "B", "O", "Y" },
                new() { "#", "#", "#", "#", "#", "C", "A", "R", "#", "#" },
                new() { "#", "#", "R", "U", "N", "#", "#", "#", "#", "#" },
                new() { "#", "#", "E", "#", "#", "B", "I", "R", "D", "#" },
                new() { "#", "#", "D", "#", "#", "#", "#", "#", "#", "#" }
            },
            Clues = new CrosswordClues
            {
                Across = new List<Clue>
                {
                    new() { Number = 1, ClueText = "Furry pet that meows", Answer = "CAT", Row = 0, Col = 0 },
                    new() { Number = 5, ClueText = "Barking pet", Answer = "DOG", Row = 0, Col = 4 },
                    new() { Number = 8, ClueText = "Sleep vision", Answer = "DREAM", Row = 2, Col = 0 },
                    new() { Number = 10, ClueText = "In addition; too", Answer = "ALSO", Row = 2, Col = 6 },
                    new() { Number = 12, ClueText = "Have fun; game", Answer = "PLAY", Row = 4, Col = 3 },
                    new() { Number = 14, ClueText = "Bright star in the sky", Answer = "SUN", Row = 5, Col = 1 },
                    new() { Number = 17, ClueText = "Male child", Answer = "BOY", Row = 5, Col = 7 },
                    new() { Number = 18, ClueText = "Automobile; vehicle", Answer = "CAR", Row = 6, Col = 5 },
                    new() { Number = 19, ClueText = "Move quickly on foot", Answer = "RUN", Row = 7, Col = 2 },
                    new() { Number = 20, ClueText = "Flying animal with wings", Answer = "BIRD", Row = 8, Col = 5 }
                },
                Down = new List<Clue>
                {
                    new() { Number = 1, ClueText = "Programming instructions", Answer = "CODE", Row = 0, Col = 0 },
                    new() { Number = 2, ClueText = "Tall plant in forest", Answer = "TREE", Row = 0, Col = 2 },
                    new() { Number = 3, ClueText = "24 hours", Answer = "DAY", Row = 0, Col = 4 },
                    new() { Number = 4, ClueText = "Opposite of old", Answer = "NEW", Row = 0, Col = 6 },
                    new() { Number = 6, ClueText = "Opposite of on", Answer = "OFF", Row = 2, Col = 1 },
                    new() { Number = 7, ClueText = "Goat, sheep, or cow", Answer = "ANIMAL", Row = 2, Col = 6 },
                    new() { Number = 9, ClueText = "Opposite of high", Answer = "LOW", Row = 2, Col = 4 },
                    new() { Number = 11, ClueText = "Not before", Answer = "AFTER", Row = 2, Col = 8 },
                    new() { Number = 13, ClueText = "Not old", Answer = "YOUNG", Row = 4, Col = 6 },
                    new() { Number = 15, ClueText = "Opposite of down", Answer = "UP", Row = 5, Col = 2 },
                    new() { Number = 16, ClueText = "Not out", Answer = "IN", Row = 5, Col = 3 },
                    new() { Number = 19, ClueText = "Color of the sky", Answer = "RED", Row = 7, Col = 2 }
                }
            }
        };

        // Puzzle 2 - A different puzzle
        puzzles["puzzle2"] = new CrosswordPuzzle
        {
            Id = "puzzle2",
            Title = "Animals & Nature",
            Size = new PuzzleSize { Rows = 8, Cols = 8 },
            Grid = new List<List<string>>
            {
                new() { "F", "I", "S", "H", "#", "#", "#", "#" },
                new() { "O", "#", "#", "O", "#", "B", "E", "E" },
                new() { "X", "#", "T", "R", "E", "E", "#", "#" },
                new() { "#", "L", "I", "O", "N", "#", "#", "#" },
                new() { "#", "#", "D", "#", "#", "F", "R", "O", "G" },
                new() { "#", "#", "E", "#", "#", "#", "#", "#" },
                new() { "B", "E", "A", "R", "#", "#", "#", "#" },
                new() { "#", "#", "#", "#", "#", "#", "#", "#" }
            },
            Clues = new CrosswordClues
            {
                Across = new List<Clue>
                {
                    new() { Number = 1, ClueText = "Swims in water", Answer = "FISH", Row = 0, Col = 0 },
                    new() { Number = 5, ClueText = "Makes honey", Answer = "BEE", Row = 1, Col = 5 },
                    new() { Number = 6, ClueText = "Has leaves and branches", Answer = "TREE", Row = 2, Col = 2 },
                    new() { Number = 8, ClueText = "King of the jungle", Answer = "LION", Row = 3, Col = 1 },
                    new() { Number = 9, ClueText = "Jumps and croaks", Answer = "FROG", Row = 4, Col = 5 },
                    new() { Number = 10, ClueText = "Large furry animal", Answer = "BEAR", Row = 6, Col = 0 }
                },
                Down = new List<Clue>
                {
                    new() { Number = 1, ClueText = "Sly animal", Answer = "FOX", Row = 0, Col = 0 },
                    new() { Number = 2, ClueText = "Water flows in it", Answer = "TIDE", Row = 2, Col = 2 },
                    new() { Number = 3, ClueText = "Barks at strangers", Answer = "HOUND", Row = 0, Col = 3 },
                    new() { Number = 4, ClueText = "Slithers on ground", Answer = "SNAKE", Row = 0, Col = 7 },
                    new() { Number = 7, ClueText = "Produces eggs", Answer = "HEN", Row = 2, Col = 4 }
                }
            }
        };

        return puzzles;
    }
}
