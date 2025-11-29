using CrossWords.Services.Models;

namespace CrossWords.Services.Abstractions;

public interface IPuzzleRepositoryReader
{
    IEnumerable<CrosswordPuzzle> LoadAllPuzzles();
}
