using CrossWords.Services.Models;
using CrossWords.Services.Abstractions;

namespace CrossWords.Services;

internal class UserProgressService : IUserProgressService
{
    private readonly ICrosswordService _crosswordService;
    private readonly IUserProgressRepositoryReader _repositoryReader;
    private readonly IUserProgressRepositoryWriter _repositoryWriter;

    public UserProgressService(ICrosswordService crosswordService, IUserProgressRepositoryReader repositoryReader, IUserProgressRepositoryWriter repositoryWriter)
    {
        _crosswordService = crosswordService ?? throw new ArgumentNullException(nameof(crosswordService));
        _repositoryReader = repositoryReader ?? throw new ArgumentNullException(nameof(repositoryReader));
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
    }

    public UserProgress GetUserProgress(string userId)
    {
        var solvedIds = _repositoryReader.GetSolvedPuzzles(userId).ToList();
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
        _repositoryWriter.RecordSolvedPuzzle(userId, puzzleId);
    }

    public AvailablePuzzlesResponse GetAvailablePuzzles(string userId, PuzzleLanguage? language = null)
    {
        var allPuzzles = _crosswordService.GetAvailablePuzzleIds(language);
        var solvedPuzzles = _repositoryReader.GetSolvedPuzzles(userId).ToList();

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
        return _repositoryReader.IsPuzzleSolved(userId, puzzleId);
    }
}
