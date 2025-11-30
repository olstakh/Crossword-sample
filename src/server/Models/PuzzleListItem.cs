using CrossWords.Services.Models;

namespace CrossWords.Models;

/// <summary>
/// Represents a puzzle in the list with its solved status
/// </summary>
public class PuzzleListItem
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public PuzzleLanguage Language { get; init; }
    public PuzzleSize Size { get; init; } = new();
    public bool IsSolved { get; init; }
}
