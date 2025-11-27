using CrossWords.Services.Models;

namespace CrossWords.Services.Abstractions;

public interface ICrosswordService
{
    CrosswordPuzzle GetPuzzle(string id);
    CrosswordPuzzle GetPuzzle(PuzzleRequest request);
    IReadOnlyList<string> GetAvailablePuzzleIds(PuzzleLanguage? language = null);
}
