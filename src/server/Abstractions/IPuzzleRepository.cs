using CrossWords.Models;

namespace CrossWords.Abstractions;

public interface IPuzzleRepository
{
    IEnumerable<CrosswordPuzzle> LoadAllPuzzles();
}
