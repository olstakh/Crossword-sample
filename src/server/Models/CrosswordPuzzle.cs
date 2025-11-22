namespace CrossWords.Models;

public class CrosswordPuzzle
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PuzzleSize Size { get; set; } = new();
    public List<List<string>> Grid { get; set; } = new();
    public CrosswordClues Clues { get; set; } = new();
}

public class PuzzleSize
{
    public int Rows { get; set; }
    public int Cols { get; set; }
}

public class CrosswordClues
{
    public List<Clue> Across { get; set; } = new();
    public List<Clue> Down { get; set; } = new();
}

public class Clue
{
    public int Number { get; set; }
    public string ClueText { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Col { get; set; }
}
