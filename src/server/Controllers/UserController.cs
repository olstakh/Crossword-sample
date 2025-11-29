using Microsoft.AspNetCore.Mvc;
using CrossWords.Models;
using CrossWords.Services.Models;
using CrossWords.Services;
using CrossWords.Services.Abstractions;

namespace CrossWords.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserProgressRepositoryReader _repositoryReader;
    private readonly IUserProgressRepositoryWriter _repositoryWriter;
    private readonly IPuzzleRepositoryReader _puzzleRepositoryReader;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserProgressRepositoryReader repositoryReader,
        IUserProgressRepositoryWriter repositoryWriter,
        IPuzzleRepositoryReader puzzleRepositoryReader,
        ILogger<UserController> logger)
    {
        _repositoryReader = repositoryReader ?? throw new ArgumentNullException(nameof(repositoryReader));
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
        _puzzleRepositoryReader = puzzleRepositoryReader ?? throw new ArgumentNullException(nameof(puzzleRepositoryReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get user's progress and solved puzzles
    /// </summary>
    [HttpGet("progress/{userId}")]
    public ActionResult<UserProgress> GetProgress(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "User ID is required" });
        }

        var solvedIds = _repositoryReader.GetSolvedPuzzles(userId).ToList();
        return Ok(new UserProgress
        {
            UserId = userId,
            SolvedPuzzleIds = solvedIds.ToList(),
            TotalPuzzlesSolved = solvedIds.Count,
            LastPlayed = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Record that user solved a puzzle
    /// </summary>
    [HttpPost("solved")]
    public ActionResult RecordSolvedPuzzle([FromBody] RecordSolvedPuzzleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.PuzzleId))
        {
            return BadRequest(new { error = "User ID and Puzzle ID are required" });
        }

        _repositoryWriter.RecordSolvedPuzzle(request.UserId, request.PuzzleId);
        _logger.LogInformation("User {UserId} solved puzzle {PuzzleId}", request.UserId, request.PuzzleId);
        
        return Ok(new { success = true, message = "Puzzle marked as solved" });
    }

    /// <summary>
    /// Get available puzzles for user (excluding solved ones)
    /// </summary>
    [HttpGet("available/{userId}")]
    public ActionResult<AvailablePuzzlesResponse> GetAvailablePuzzles(
        string userId,
        [FromQuery] PuzzleLanguage? language = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "User ID is required" });
        }

        var allPuzzles = _puzzleRepositoryReader
            .GetPuzzles(language: language, sizeCategory: PuzzleSizeCategory.Any)
            .Select(p => p.Id)
            .ToList();
        var solvedPuzzles = _repositoryReader.GetSolvedPuzzles(userId).ToList();

        var unsolvedPuzzles = allPuzzles
            .Where(id => !solvedPuzzles.Contains(id))
            .ToList();

        return Ok(new AvailablePuzzlesResponse
        {
            UnsolvedPuzzleIds = unsolvedPuzzles,
            SolvedPuzzleIds = solvedPuzzles.Where(id => allPuzzles.Contains(id)).ToList(),
            TotalAvailable = allPuzzles.Count,
            TotalSolved = solvedPuzzles.Count(id => allPuzzles.Contains(id))
        });
    }

    /// <summary>
    /// Check if user has solved a specific puzzle
    /// </summary>
    [HttpGet("has-solved/{userId}/{puzzleId}")]
    public ActionResult<bool> HasSolvedPuzzle(string userId, string puzzleId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(puzzleId))
        {
            return BadRequest(new { error = "User ID and Puzzle ID are required" });
        }

        var hasSolved = _repositoryReader.IsPuzzleSolved(userId, puzzleId);
        return Ok(new { hasSolved });
    }
}
