using CrossWords.Services.Models;

namespace CrossWords.Services.Abstractions;

public interface IUserProgressRepositoryWriter
{
    /// <summary>
    /// Record that user has solved a puzzle
    /// </summary>
    void RecordSolvedPuzzle(string userId, PuzzleId puzzleId);

    /// <summary>
    /// Remove solved puzzle records for a user
    /// </summary>
    void ForgetPuzzles(string userId, IEnumerable<PuzzleId> puzzleIds);

    /// <summary>
    /// Import user progress records (replaces existing data)
    /// </summary>
    void ImportUserProgress(IEnumerable<UserProgressRecord> records);
}