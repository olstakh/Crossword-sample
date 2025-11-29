using CrossWords.Services.Models;

namespace CrossWords.Services.Abstractions;

/// <summary>
/// Repository interface for persisting puzzles (add/delete operations)
/// Separate from IPuzzleRepository to maintain read-only interface for services that don't need write access
/// </summary>
public interface IPuzzleRepositoryWriter
{
    /// <summary>
    /// Add a new puzzle to the repository (or update if it already exists)
    /// </summary>
    void AddPuzzle(CrosswordPuzzle puzzle);
    
    /// <summary>
    /// Add multiple puzzles to the repository in bulk (or update if they already exist)
    /// </summary>
    void AddPuzzles(IEnumerable<CrosswordPuzzle> puzzles);
    
    /// <summary>
    /// Delete a puzzle from the repository
    /// </summary>
    void DeletePuzzle(string puzzleId);
}
