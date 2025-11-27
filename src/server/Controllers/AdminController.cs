using CrossWords.Models;
using CrossWords.Abstractions;
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
        puzzle.Validate();

        _puzzlePersister.AddPuzzle(puzzle);
        _logger.LogInformation("Successfully added puzzle {PuzzleId} via admin API", puzzle.Id);
        
        return Ok(new { message = "Puzzle added successfully", puzzleId = puzzle.Id });
    }

    /// <summary>
    /// Delete a puzzle from the database
    /// </summary>
    [HttpDelete("puzzles/{puzzleId}")]
    public IActionResult DeletePuzzle(string puzzleId)
    {
        if (string.IsNullOrEmpty(puzzleId))
        {
            return BadRequest(new { error = "Puzzle ID is required" });
        }

        _puzzlePersister.DeletePuzzle(puzzleId);
        _logger.LogInformation("Successfully deleted puzzle {PuzzleId} via admin API", puzzleId);
        return Ok(new { message = "Puzzle deleted successfully", puzzleId });
    }

    /// <summary>
    /// Delete multiple puzzles from the database
    /// </summary>
    [HttpPost("puzzles/delete-bulk")]
    public IActionResult DeletePuzzles([FromBody] DeletePuzzlesRequest request)
    {
        if (request.PuzzleIds == null || request.PuzzleIds.Count == 0)
        {
            return BadRequest(new { error = "At least one puzzle ID is required" });
        }

        var deletedIds = new List<string>();
        var errors = new List<string>();

        foreach (var puzzleId in request.PuzzleIds)
        {
            try
            {
                _puzzlePersister.DeletePuzzle(puzzleId);
                deletedIds.Add(puzzleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting puzzle {PuzzleId}", puzzleId);
                errors.Add($"{puzzleId}: {ex.Message}");
            }
        }

        _logger.LogInformation("Bulk deleted {Count} puzzles via admin API", deletedIds.Count);
        
        return Ok(new 
        { 
            message = $"Successfully deleted {deletedIds.Count} puzzle(s)", 
            deletedIds, 
            errors 
        });
    }

    /// <summary>
    /// Get all puzzles (for admin view)
    /// </summary>
    [HttpGet("puzzles")]
    public IActionResult GetAllPuzzles()
    {
        var puzzles = _puzzleRepository.LoadAllPuzzles();
        return Ok(puzzles);
    }
}
