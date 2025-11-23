using Microsoft.AspNetCore.Mvc;
using CrossWords.Services;
using CrossWords.Models;

namespace CrossWords.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrosswordController : ControllerBase
{
    private readonly ICrosswordService _crosswordService;
    private readonly ILogger<CrosswordController> _logger;

    public CrosswordController(ICrosswordService crosswordService, ILogger<CrosswordController> logger)
    {
        _crosswordService = crosswordService;
        _logger = logger;
    }

    /// <summary>
    /// Get a list of available puzzle IDs
    /// </summary>
    [HttpGet("puzzles")]
    public ActionResult<List<string>> GetPuzzleList()
    {
        try
        {
            var puzzleIds = _crosswordService.GetAvailablePuzzleIds();
            return Ok(puzzleIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving puzzle list");
            return StatusCode(500, "Error retrieving puzzle list");
        }
    }

    /// <summary>
    /// Get a specific puzzle by ID
    /// </summary>
    [HttpGet("puzzle/{id}")]
    public ActionResult<CrosswordPuzzle> GetPuzzle(string id)
    {
        try
        {
            var puzzle = _crosswordService.GetPuzzle(id);
            if (puzzle == null)
            {
                return NotFound($"Puzzle with ID '{id}' not found");
            }
            return Ok(puzzle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving puzzle {PuzzleId}", id);
            return StatusCode(500, "Error retrieving puzzle");
        }
    }

    /// <summary>
    /// Get the default puzzle (puzzle1)
    /// </summary>
    [HttpGet("puzzle")]
    public ActionResult<CrosswordPuzzle> GetDefaultPuzzle()
    {
        return GetPuzzle("puzzle2");
    }

    /// <summary>
    /// Get a puzzle by size (small, medium, or big)
    /// </summary>
    /// <param name="size">Size of puzzle: small (5x5-8x8), medium (9x9-14x14), or big (15x15-20x20)</param>
    /// <param name="seed">Optional seed for deterministic generation. If not provided, uses current date.</param>
    [HttpGet("puzzle/size/{size}")]
    public ActionResult<CrosswordPuzzle> GetPuzzleBySize(string size, [FromQuery] string? seed = null)
    {
        try
        {
            var validSizes = new[] { "small", "medium", "big" };
            if (!validSizes.Contains(size.ToLower()))
            {
                return BadRequest($"Invalid size. Must be one of: {string.Join(", ", validSizes)}");
            }

            var puzzle = _crosswordService.GetPuzzleBySize(size, seed);
            return Ok(puzzle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating puzzle of size {Size}", size);
            return StatusCode(500, "Error generating puzzle");
        }
    }
}
