using CrossWords.Models;
using CrossWords.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrossWords.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IPuzzleRepository _puzzleRepository;
    private readonly IPuzzleRepositoryPersister _puzzlePersister;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IPuzzleRepository puzzleRepository, 
        IPuzzleRepositoryPersister puzzlePersister,
        ILogger<AdminController> logger)
    {
        _puzzleRepository = puzzleRepository;
        _puzzlePersister = puzzlePersister;
        _logger = logger;
    }

    /// <summary>
    /// Add a new puzzle to the database
    /// </summary>
    [HttpPost("puzzles")]
    public IActionResult AddPuzzle([FromBody] CrosswordPuzzle puzzle)
    {
        try
        {
            puzzle.Validate();

            _puzzlePersister.AddPuzzle(puzzle);
            _logger.LogInformation("Successfully added puzzle {PuzzleId} via admin API", puzzle.Id);
            
            return Ok(new { message = "Puzzle added successfully", puzzleId = puzzle.Id });
        }
        catch (PuzzleValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error adding puzzle via admin API");
            return BadRequest(new { error = "Puzzle validation failed", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding puzzle via admin API");
            return StatusCode(500, new { error = "Failed to add puzzle", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a puzzle from the database
    /// </summary>
    [HttpDelete("puzzles/{puzzleId}")]
    public IActionResult DeletePuzzle(string puzzleId)
    {
        try
        {
            if (string.IsNullOrEmpty(puzzleId))
            {
                return BadRequest(new { error = "Puzzle ID is required" });
            }

            _puzzlePersister.DeletePuzzle(puzzleId);
            _logger.LogInformation("Successfully deleted puzzle {PuzzleId} via admin API", puzzleId);
            return Ok(new { message = "Puzzle deleted successfully", puzzleId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting puzzle via admin API");
            return StatusCode(500, new { error = "Failed to delete puzzle", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all puzzles (for admin view)
    /// </summary>
    [HttpGet("puzzles")]
    public IActionResult GetAllPuzzles()
    {
        try
        {
            var puzzles = _puzzleRepository.LoadAllPuzzles();
            return Ok(puzzles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading puzzles via admin API");
            return StatusCode(500, new { error = "Failed to load puzzles", details = ex.Message });
        }
    }
}
