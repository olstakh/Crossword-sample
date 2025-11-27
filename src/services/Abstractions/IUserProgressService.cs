using CrossWords.Services.Models;

namespace CrossWords.Services.Abstractions;

/// <summary>
/// Service for managing user progress and solved puzzles
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
