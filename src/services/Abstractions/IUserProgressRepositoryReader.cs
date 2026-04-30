using CrossWords.Services.Models;

namespace CrossWords.Services.Abstractions;

/// <summary>
/// Repository interface for persisting user progress
/// </summary>
public interface IUserProgressRepositoryReader
{
    /// <summary>
    /// Check if user has solved a specific puzzle
    /// </summary>
    bool IsPuzzleSolved(string userId, PuzzleId puzzleId);
        
    /// <summary>
    /// Get all solved puzzle IDs for a user
    /// </summary>
    HashSet<PuzzleId> GetSolvedPuzzles(string userId);

    /// <summary>
    /// Get all user IDs that have progress records
    /// </summary>
    IEnumerable<string> GetAllUsers();

    /// <summary>
    /// Get all user progress records (for export/backup)
    /// </summary>
    IEnumerable<UserProgressRecord> GetAllUserProgress();
}

/// <summary>
/// Represents a single user progress record for export/import
/// </summary>
public class UserProgressRecord
{
    public string UserId { get; init; } = string.Empty;
    public PuzzleId PuzzleId { get; init; }
    public DateTime SolvedAt { get; init; }
}
