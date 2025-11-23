namespace CrossWords.Models;

/// <summary>
/// Represents a user's progress and solved puzzles
/// </summary>
public class UserProgress
{
    public string UserId { get; init; } = string.Empty;
    public List<string> SolvedPuzzleIds { get; init; } = new();
    public DateTime LastPlayed { get; init; } = DateTime.UtcNow;
    public int TotalPuzzlesSolved { get; init; }
}

/// <summary>
/// Request to record a solved puzzle
/// </summary>
public class RecordSolvedPuzzleRequest
{
    public string PuzzleId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Response with available puzzles excluding solved ones
/// </summary>
public class AvailablePuzzlesResponse
{
    public List<string> UnsolvedPuzzleIds { get; init; } = new();
    public List<string> SolvedPuzzleIds { get; init; } = new();
    public int TotalAvailable { get; init; }
    public int TotalSolved { get; init; }
}
