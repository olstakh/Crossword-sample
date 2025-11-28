using CrossWords.Services.Models;

namespace CrossWords.Services.Abstractions;

public interface IPuzzleRepository
{
    IEnumerable<CrosswordPuzzle> LoadAllPuzzles();
}
