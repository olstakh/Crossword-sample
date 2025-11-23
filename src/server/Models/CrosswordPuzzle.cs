using System.Text.Json.Serialization;

namespace CrossWords.Models;

public class CrosswordPuzzle
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public PuzzleLanguage Language { get; init; } = PuzzleLanguage.English;
    public PuzzleSize Size { get; init; } = new();
    public List<List<string>> Grid { get; init; } = new(); // Letters in the solution, "#" for black cells
}

public class PuzzleSize
{
    public int Rows { get; init; }
    public int Cols { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PuzzleLanguage
{
    English,
    Russian,
    Ukrainian
}

public enum PuzzleSizeCategory
{
    Small,
    Medium,
    Big
}
