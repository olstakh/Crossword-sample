namespace CrossWords.Services.Abstractions;

/// <summary>
/// Repository interface for persisting user progress
/// </summary>
public interface IUserProgressRepositoryReader
{
    /// <summary>
    /// Check if user has solved a specific puzzle
    /// </summary>
    bool IsPuzzleSolved(string userId, string puzzleId);
    
    /// <summary>
    /// Record that user has solved a puzzle
    /// </summary>
    void RecordSolvedPuzzle(string userId, string puzzleId);
    
    /// <summary>
    /// Get all solved puzzle IDs for a user
    /// </summary>
    HashSet<string> GetSolvedPuzzles(string userId);
}
