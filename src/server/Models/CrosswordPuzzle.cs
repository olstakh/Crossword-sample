namespace CrossWords.Models;

public class CrosswordPuzzle
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PuzzleSize Size { get; set; } = new();
    public List<List<string>> Grid { get; set; } = new(); // Letters in the solution
    public List<List<int>> Numbers { get; set; } = new(); // Number for each cell (0 for black cells)
    public Dictionary<int, string> LetterMapping { get; set; } = new(); // Number -> Letter mapping
    public List<int> InitiallyRevealed { get; set; } = new(); // Numbers that are revealed at start
}

public class PuzzleSize
{
    public int Rows { get; set; }
    public int Cols { get; set; }
}

public enum PuzzleSizeCategory
{
    Small,
    Medium,
    Big
}
