using System.Text.Json.Serialization;
using CrossWords.Services.Exceptions;

namespace CrossWords.Services.Models;

public class CrosswordPuzzle
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public PuzzleLanguage Language { get; init; } = PuzzleLanguage.English;
    public PuzzleSize Size { get; init; } = new();
    public List<List<string>> Grid { get; init; } = new(); // Letters in the solution, "#" for black cells

    /// <exception cref="PuzzleValidationException">Thrown when the puzzle data is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new PuzzleValidationException("Puzzle ID cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(Title))
            throw new PuzzleValidationException("Puzzle title cannot be null or empty.");

        if (Size.Rows <= 0 || Size.Cols <= 0)
            throw new PuzzleValidationException("Puzzle size must have positive number of rows and columns.");

        if (Grid.Count != Size.Rows)
            throw new PuzzleValidationException($"Grid row count does not match specified size rows {Size.Rows}.");

        if (Grid.Any(row => row.Count != Size.Cols))
            throw new PuzzleValidationException($"Grid column count does not match specified size cols {Size.Cols}.");
    }
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
    Big,
    Any
}

public class PuzzleRequest
{
    public PuzzleSizeCategory SizeCategory { get; init; } = PuzzleSizeCategory.Any;
    public PuzzleLanguage Language { get; init; } = PuzzleLanguage.English;
    public string? UserId { get; init; }
}

internal static class PuzzleSizeCategoryExtensions
{
    public static (int minSize, int maxSize) GetSizeRange(this PuzzleSizeCategory sizeCategory)
    {
        return sizeCategory switch
        {
            PuzzleSizeCategory.Any => (1, 1000),
            PuzzleSizeCategory.Small => (5, 8),
            PuzzleSizeCategory.Medium => (9, 14),
            PuzzleSizeCategory.Big => (15, 20),
            _ => throw new ArgumentOutOfRangeException(nameof(sizeCategory), "Invalid puzzle size category")
        };
    }
}
