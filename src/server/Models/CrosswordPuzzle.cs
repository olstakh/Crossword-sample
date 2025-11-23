namespace CrossWords.Models;

public class CrosswordPuzzle
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PuzzleSize Size { get; set; } = new();
    public List<List<string>> Grid { get; set; } = new(); // Letters in the solution, "#" for black cells
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
