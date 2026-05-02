using CrossWords.Services.Models;

namespace CrossWords.Models;

/// <summary>
/// Request to record a solved puzzle
/// </summary>
public class RecordSolvedPuzzleRequest
{
    public PuzzleId PuzzleId { get; init; }
}

/// <summary>
/// Request to delete multiple puzzles
/// </summary>
public class DeletePuzzlesRequest
{
    public List<PuzzleId> PuzzleIds { get; init; } = new();
}

/// <summary>
/// Request to forget (mark as unsolved) multiple puzzles for a user
/// </summary>
public class ForgetPuzzlesRequest
{
    public IEnumerable<PuzzleId> PuzzleIds { get; init; } = new List<PuzzleId>();
}
