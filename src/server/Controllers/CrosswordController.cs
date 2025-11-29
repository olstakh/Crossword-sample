using Microsoft.AspNetCore.Mvc;
using CrossWords.Services;
using CrossWords.Services.Abstractions;
using CrossWords.Services.Models;
using CrossWords.Services.Exceptions;

namespace CrossWords.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrosswordController : ControllerBase
{
    private readonly IPuzzleRepositoryReader _puzzleRepositoryReader;
    private readonly IUserProgressRepositoryReader _userProgressRepositoryReader;
    private readonly ILogger<CrosswordController> _logger;

    public CrosswordController(IPuzzleRepositoryReader puzzleRepositoryReader, IUserProgressRepositoryReader userProgressRepositoryReader, ILogger<CrosswordController> logger)
    {
        _puzzleRepositoryReader = puzzleRepositoryReader ?? throw new ArgumentNullException(nameof(puzzleRepositoryReader));
        _userProgressRepositoryReader = userProgressRepositoryReader ?? throw new ArgumentNullException(nameof(puzzleRepositoryReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a list of available puzzle IDs
    /// </summary>
    [HttpGet("puzzles")]
    public ActionResult<List<string>> GetPuzzleList([FromQuery] PuzzleLanguage? language = null)
    {
        var puzzleIds = _puzzleRepositoryReader
            .GetPuzzles(language: language, sizeCategory: PuzzleSizeCategory.Any)
            .Select(p => p.Id)
            .ToList();

        return Ok(puzzleIds);
    }

    /// <summary>
    /// Get a specific puzzle by ID
    /// </summary>
    [HttpGet("puzzle/{id}")]
    public ActionResult<CrosswordPuzzle> GetPuzzle(string id)
    {
        var puzzle = _puzzleRepositoryReader.GetPuzzle(id);

        if (puzzle == null)
        {
            throw new PuzzleNotFoundException($"Puzzle with ID '{id}' was not found.");
        }

        return Ok(puzzle);
    }

    /// <summary>
    /// Get a puzzle by size (Small, Medium, or Big)
    /// </summary>
    /// <param name="size">Size of puzzle: Small (5x5-8x8), Medium (9x9-14x14), Big (15x15-20x20), or Any (all sizes)</param>
    /// <param name="language">Language of the puzzle (English, Russian, Ukrainian)</param>
    /// <param name="seed">Optional seed for deterministic generation. If not provided, uses current date.</param>
    [HttpGet("puzzle")]
    public ActionResult<CrosswordPuzzle> GetPuzzle(
        [FromQuery] PuzzleSizeCategory size = PuzzleSizeCategory.Any, 
        [FromQuery] PuzzleLanguage language = PuzzleLanguage.English, 
        [FromQuery] string? seed = null,
        [FromHeader(Name = "X-User-Id")] string? userId = null)
    {
        var request = new PuzzleRequest
        {
            SizeCategory = size,
            Language = language,
            UserId = userId
        };     
        
        var puzzles = _puzzleRepositoryReader
            .GetPuzzles(sizeCategory: size, language: language)
            .Where(p => userId == null || !_userProgressRepositoryReader.IsPuzzleSolved(userId, p.Id))
            .ToList();

        if (puzzles.Count == 0)
        {
            // Should this be a 404 instead?
            throw new PuzzleNotFoundException(
                $"No puzzles found for language '{language}' and size '{size}', you probably solved all. Please try a different combination.");
        }
        return Ok(puzzles[Random.Shared.Next(puzzles.Count)]);
    }
}
