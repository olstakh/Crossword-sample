using CrossWords.Models;

namespace CrossWords.Services;

/// <summary>
/// Repository interface for persisting puzzles (add/delete operations)
/// Separate from IPuzzleRepository to maintain read-only interface for services that don't need write access
/// </summary>
public interface IPuzzleRepositoryPersister
{
    /// <summary>
    /// Add a new puzzle to the repository (or update if it already exists)
    /// </summary>
    void AddPuzzle(CrosswordPuzzle puzzle);
    
    /// <summary>
    /// Delete a puzzle from the repository
    /// </summary>
    void DeletePuzzle(string puzzleId);
}
