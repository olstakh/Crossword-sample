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

/// <summary>
/// Request to forget (mark as unsolved) multiple puzzles for a user
/// </summary>
public class ForgetPuzzlesRequest
{
    public IEnumerable<string> PuzzleIds { get; init; } = new List<string>();
}
