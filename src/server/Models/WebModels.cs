namespace CrossWords.Models;

/// <summary>
/// Request to record a solved puzzle
/// </summary>
public class RecordSolvedPuzzleRequest
{
    public string PuzzleId { get; init; } = string.Empty;
}

/// <summary>
/// Request to delete multiple puzzles
/// </summary>
public class DeletePuzzlesRequest
{
    public List<string> PuzzleIds { get; init; } = new();
}
