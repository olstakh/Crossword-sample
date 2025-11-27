using CrossWords.Models;

namespace CrossWords.Abstractions;

public interface ICrosswordService
{
    CrosswordPuzzle GetPuzzle(string id);
    CrosswordPuzzle GetPuzzle(PuzzleRequest request);
    IReadOnlyList<string> GetAvailablePuzzleIds(PuzzleLanguage? language = null);
}
