using CrossWords.Models;
using CrossWords.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrossWords.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IPuzzleRepository _puzzleRepository;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IPuzzleRepository puzzleRepository, ILogger<AdminController> logger)
    {
        _puzzleRepository = puzzleRepository;
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
            if (string.IsNullOrEmpty(puzzle.Id))
            {
                return BadRequest(new { error = "Puzzle ID is required" });
            }

            if (string.IsNullOrEmpty(puzzle.Title))
            {
                return BadRequest(new { error = "Puzzle title is required" });
            }

            if (puzzle.Grid == null || !puzzle.Grid.Any())
            {
                return BadRequest(new { error = "Puzzle grid is required" });
            }

            // Validate grid dimensions match Size
            if (puzzle.Grid.Count != puzzle.Size.Rows)
            {
                return BadRequest(new { error = "Grid row count doesn't match Size.Rows" });
            }

            if (puzzle.Grid.Any(row => row.Count != puzzle.Size.Cols))
            {
                return BadRequest(new { error = "Grid column count doesn't match Size.Cols" });
            }

            // Check if repository supports adding puzzles
            if (_puzzleRepository is SqlitePuzzleRepository sqliteRepo)
            {
                sqliteRepo.AddPuzzle(puzzle);
                _logger.LogInformation("Successfully added puzzle {PuzzleId} via admin API", puzzle.Id);
                return Ok(new { message = "Puzzle added successfully", puzzleId = puzzle.Id });
            }
            else
            {
                return StatusCode(501, new { error = "Current repository does not support adding puzzles. Please use SQLite repository." });
            }
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

            // Check if repository supports deleting puzzles
            if (_puzzleRepository is SqlitePuzzleRepository sqliteRepo)
            {
                sqliteRepo.DeletePuzzle(puzzleId);
                _logger.LogInformation("Successfully deleted puzzle {PuzzleId} via admin API", puzzleId);
                return Ok(new { message = "Puzzle deleted successfully", puzzleId });
            }
            else
            {
                return StatusCode(501, new { error = "Current repository does not support deleting puzzles. Please use SQLite repository." });
            }
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
