using Microsoft.AspNetCore.Mvc;
using CrossWords.Models;
using CrossWords.Services;

namespace CrossWords.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserProgressService _userProgressService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserProgressService userProgressService, ILogger<UserController> logger)
    {
        _userProgressService = userProgressService;
        _logger = logger;
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

        var progress = _userProgressService.GetUserProgress(userId);
        return Ok(progress);
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

        _userProgressService.RecordSolvedPuzzle(request.UserId, request.PuzzleId);
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

        var response = _userProgressService.GetAvailablePuzzles(userId, language);
        return Ok(response);
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

        var hasSolved = _userProgressService.HasSolvedPuzzle(userId, puzzleId);
        return Ok(new { hasSolved });
    }
}
