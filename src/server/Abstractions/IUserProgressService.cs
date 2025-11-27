using CrossWords.Models;

namespace CrossWords.Abstractions;

/// <summary>
/// Service for managing user progress and solved puzzles
/// Uses in-memory storage (can be replaced with database later)
/// </summary>
public interface IUserProgressService
{
    /// <summary>
    /// Get user's progress including solved puzzles
    /// </summary>
    UserProgress GetUserProgress(string userId);
    
    /// <summary>
    /// Record that a user has solved a puzzle
    /// </summary>
    void RecordSolvedPuzzle(string userId, string puzzleId);
    
    /// <summary>
    /// Get available puzzles for user (excluding already solved)
    /// </summary>
    AvailablePuzzlesResponse GetAvailablePuzzles(string userId, PuzzleLanguage? language = null);
    
    /// <summary>
    /// Check if user has solved a specific puzzle
    /// </summary>
    bool HasSolvedPuzzle(string userId, string puzzleId);
}
