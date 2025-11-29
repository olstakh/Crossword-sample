namespace CrossWords.Services.Abstractions;

public interface IUserProgressRepositoryWriter
{
    /// <summary>
    /// Record that user has solved a puzzle
    /// </summary>
    void RecordSolvedPuzzle(string userId, string puzzleId);

    /// <summary>
    /// Remove solved puzzle records for a user
    /// </summary>
    void ForgetPuzzles(string userId, IEnumerable<string> puzzleIds);
}