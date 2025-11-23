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

public class PuzzleRequest
{
    public PuzzleSizeCategory SizeCategory { get; init; } = PuzzleSizeCategory.Medium;
    public PuzzleLanguage Language { get; init; } = PuzzleLanguage.English;
}

internal static class PuzzleSizeCategoryExtensions
{
    public static (int minSize, int maxSize) GetSizeRange(this PuzzleSizeCategory sizeCategory)
    {
        return sizeCategory switch
        {
            PuzzleSizeCategory.Small => (5, 8),
            PuzzleSizeCategory.Medium => (9, 14),
            PuzzleSizeCategory.Big => (15, 20),
            _ => throw new ArgumentOutOfRangeException(nameof(sizeCategory), "Invalid puzzle size category")
        };
    }
}
