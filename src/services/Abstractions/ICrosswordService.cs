using CrossWords.Services.Models;

namespace CrossWords.Services.Abstractions;

public interface ICrosswordService
{
    CrosswordPuzzle GetPuzzle(string id);
    IEnumerable<CrosswordPuzzle> GetPuzzles(PuzzleRequest request);
    IReadOnlyList<string> GetAvailablePuzzleIds(PuzzleLanguage? language = null);
}
