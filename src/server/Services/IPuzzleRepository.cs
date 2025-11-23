using CrossWords.Models;

namespace CrossWords.Services;

public interface IPuzzleRepository
{
    IEnumerable<CrosswordPuzzle> LoadAllPuzzles();
}
