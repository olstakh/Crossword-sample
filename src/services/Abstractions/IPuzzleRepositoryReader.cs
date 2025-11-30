using CrossWords.Services.Models;

namespace CrossWords.Services.Abstractions;

public interface IPuzzleRepositoryReader
{
    IEnumerable<CrosswordPuzzle> LoadAllPuzzles();

    /// <summary>
    /// Get all the puzzles satisfying given parameters.
    /// </summary>
    /// <param name="sizeCategory">Puzzle size category.</param>
    /// <param name="language">Puzzle language (null means any language)</param>
    /// <returns>Collection of puzzles</returns>
    IEnumerable<CrosswordPuzzle> GetPuzzles(PuzzleSizeCategory sizeCategory = PuzzleSizeCategory.Any, PuzzleLanguage? language = null);

    /// <summary>
    /// Gets puzzle by given puzzle id
    /// </summary>
    /// <param name="puzzleId">Id of a puzzle to look up.</param>
    /// <returns>Puzzle or <c>null</c> if given id not found.</returns>
    CrosswordPuzzle? GetPuzzle(string puzzleId);
}
