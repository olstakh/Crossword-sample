using CrossWords.Services.Models;
using CrossWords.Services.Abstractions;

namespace CrossWords.Services;

internal class UserProgressService : IUserProgressService
{
    private readonly ICrosswordService _crosswordService;
    private readonly IUserProgressRepositoryReader _repository;

    public UserProgressService(ICrosswordService crosswordService, IUserProgressRepositoryReader repository)
    {
        _crosswordService = crosswordService ?? throw new ArgumentNullException(nameof(crosswordService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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
