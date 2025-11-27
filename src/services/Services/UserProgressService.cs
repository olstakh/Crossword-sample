using CrossWords.Services.Models;
using CrossWords.Services.Abstractions;

namespace CrossWords.Services;

public class UserProgressService : IUserProgressService
{
    private readonly ICrosswordService _crosswordService;
    private readonly IUserProgressRepository _repository;

    public UserProgressService(ICrosswordService crosswordService, IUserProgressRepository repository)
    {
        _crosswordService = crosswordService;
        _repository = repository;
    }

    public UserProgress GetUserProgress(string userId)
    {
        var solvedIds = _repository.GetSolvedPuzzles(userId).ToList();
        var availablePuzzleIds = _crosswordService.GetAvailablePuzzleIds();
        return new UserProgress
        {
            UserId = userId,
            SolvedPuzzleIds = solvedIds.Intersect(availablePuzzleIds).ToList(),
            TotalPuzzlesSolved = solvedIds.Count,
            LastPlayed = DateTime.UtcNow
        };
    }

    public void RecordSolvedPuzzle(string userId, string puzzleId)
    {
        _repository.RecordSolvedPuzzle(userId, puzzleId);
    }

    public AvailablePuzzlesResponse GetAvailablePuzzles(string userId, PuzzleLanguage? language = null)
    {
        var allPuzzles = _crosswordService.GetAvailablePuzzleIds(language);
        var solvedPuzzles = _repository.GetSolvedPuzzles(userId).ToList();

        var unsolvedPuzzles = allPuzzles
            .Where(id => !solvedPuzzles.Contains(id))
            .ToList();

        return new AvailablePuzzlesResponse
        {
            UnsolvedPuzzleIds = unsolvedPuzzles,
            SolvedPuzzleIds = solvedPuzzles.Where(id => allPuzzles.Contains(id)).ToList(),
            TotalAvailable = allPuzzles.Count,
            TotalSolved = solvedPuzzles.Count(id => allPuzzles.Contains(id))
        };
    }

    public bool HasSolvedPuzzle(string userId, string puzzleId)
    {
        return _repository.IsPuzzleSolved(userId, puzzleId);
    }
}
