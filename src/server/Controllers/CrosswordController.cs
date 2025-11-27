using Microsoft.AspNetCore.Mvc;
using CrossWords.Services;
using CrossWords.Models;
using CrossWords.Exceptions;

namespace CrossWords.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrosswordController : ControllerBase
{
    private readonly ICrosswordService _crosswordService;
    private readonly ILogger<CrosswordController> _logger;

    public CrosswordController(ICrosswordService crosswordService, ILogger<CrosswordController> logger)
    {
        _crosswordService = crosswordService ?? throw new ArgumentNullException(nameof(crosswordService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a list of available puzzle IDs
    /// </summary>
    [HttpGet("puzzles")]
    public ActionResult<List<string>> GetPuzzleList([FromQuery] PuzzleLanguage? language = null)
    {
        var puzzleIds = _crosswordService.GetAvailablePuzzleIds(language);
        return Ok(puzzleIds);
    }

    /// <summary>
    /// Get a specific puzzle by ID
    /// </summary>
    [HttpGet("puzzle/{id}")]
    public ActionResult<CrosswordPuzzle> GetPuzzle(string id)
    {
        var puzzle = _crosswordService.GetPuzzle(id);
        return Ok(puzzle);
    }

    /// <summary>
    /// Get a puzzle by size (Small, Medium, or Big)
    /// </summary>
    /// <param name="size">Size of puzzle: Small (5x5-8x8), Medium (9x9-14x14), Big (15x15-20x20), or Any (all sizes)</param>
    /// <param name="language">Language of the puzzle (English, Russian, Ukrainian)</param>
    /// <param name="seed">Optional seed for deterministic generation. If not provided, uses current date.</param>
    [HttpGet("puzzle")]
    public ActionResult<CrosswordPuzzle> GetPuzzleBySize(
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
        
        var puzzle = _crosswordService.GetPuzzle(request);
        return Ok(puzzle);
    }
}
