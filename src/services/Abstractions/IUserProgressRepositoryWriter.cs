namespace CrossWords.Services.Abstractions;

public interface IUserProgressRepositoryWriter
{
    /// <summary>
    /// Record that user has solved a puzzle
    /// </summary>
    void RecordSolvedPuzzle(string userId, string puzzleId);
}