using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
    [HttpGet("progress")]
    public ActionResult<UserProgress> GetProgress([FromHeader(Name = "X-User-Id")] string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "User ID is required in X-User-Id header" });
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
    public ActionResult RecordSolvedPuzzle(
        [FromBody] RecordSolvedPuzzleRequest request,
        [FromHeader(Name = "X-User-Id")] string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "User ID is required in X-User-Id header" });
        }

        if (string.IsNullOrWhiteSpace(request.PuzzleId))
        {
            return BadRequest(new { error = "Puzzle ID is required" });
        }

        _repositoryWriter.RecordSolvedPuzzle(userId, request.PuzzleId);
        _logger.LogInformation("User {UserId} solved puzzle {PuzzleId}", userId, request.PuzzleId);
        
        return Ok(new { success = true, message = "Puzzle marked as solved" });
    }

    /// <summary>
    /// Get available puzzles for user (excluding solved ones)
    /// </summary>
    [HttpGet("available")]
    public ActionResult<AvailablePuzzlesResponse> GetAvailablePuzzles(
        [FromHeader(Name = "X-User-Id")] string? userId,
        [FromQuery] PuzzleLanguage? language = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "User ID is required in X-User-Id header" });
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
    [HttpGet("has-solved/{puzzleId}")]
    public ActionResult<bool> HasSolvedPuzzle(
        [FromHeader(Name = "X-User-Id")] string? userId,
        string puzzleId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "User ID is required in X-User-Id header" });
        }

        if (string.IsNullOrWhiteSpace(puzzleId))
        {
            return BadRequest(new { error = "Puzzle ID is required" });
        }

        var hasSolved = _repositoryReader.IsPuzzleSolved(userId, puzzleId);
        return Ok(new { hasSolved });
    }

    /// <summary>
    /// Remove solved puzzle records for user (mark puzzles as unsolved)
    /// </summary>
    [HttpPost("forget")]
    public ActionResult ForgetPuzzles(
        [FromBody] ForgetPuzzlesRequest request,
        [FromHeader(Name = "X-User-Id")] string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "User ID is required in X-User-Id header" });
        }

        if (request.PuzzleIds == null || !request.PuzzleIds.Any())
        {
            return BadRequest(new { error = "At least one puzzle ID is required" });
        }

        _repositoryWriter.ForgetPuzzles(userId, request.PuzzleIds);
        _logger.LogInformation("User {UserId} forgot {Count} puzzle(s)", userId, request.PuzzleIds.Count());
        
        return Ok(new { success = true, message = $"Forgot {request.PuzzleIds.Count()} puzzle(s)" });
    }

    /// <summary>
    /// Get all users with progress records
    /// </summary>
    [HttpGet("all")]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult<IEnumerable<string>> GetAllUsers()
    {
        var users = _repositoryReader.GetAllUsers().ToList();
        return Ok(users);
    }

    /// <summary>
    /// Download all user progress data (for backup)
    /// </summary>
    [HttpGet("progress/download")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult DownloadUserProgress()
    {
        try
        {
            var records = _repositoryReader.GetAllUserProgress();
            _logger.LogInformation("Downloaded user progress data via admin API");
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading user progress");
            return StatusCode(500, new { error = "Failed to download user progress", message = ex.Message });
        }
    }

    /// <summary>
    /// Upload user progress data (replaces existing data)
    /// </summary>
    [HttpPost("progress/upload")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult UploadUserProgress([FromBody] List<UserProgressRecord> records)
    {
        if (records == null || records.Count == 0)
        {
            return BadRequest(new { error = "At least one user progress record is required" });
        }

        try
        {
            _repositoryWriter.ImportUserProgress(records);
            _logger.LogInformation("Successfully uploaded {Count} user progress records via admin API", records.Count);
            
            return Ok(new 
            { 
                message = $"Successfully uploaded {records.Count} user progress record(s)",
                count = records.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading user progress");
            return StatusCode(500, new { error = "Failed to upload user progress", message = ex.Message });
        }
    }
}
