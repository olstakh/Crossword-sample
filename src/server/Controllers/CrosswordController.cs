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
    /// Get a puzzle in the current language
    /// Language is determined from Accept-Language header.
    /// </summary>
    /// <param name="seed">Optional seed for deterministic generation. If not provided, uses current date.</param>
    [HttpGet("puzzle")]
    public ActionResult<CrosswordPuzzle> GetPuzzle(
        [FromQuery] string? seed = null,
        [FromHeader(Name = "X-User-Id")] string? userId = null,
        [FromHeader(Name = "Accept-Language")] string? acceptLanguage = null)
    {
        // Get language from Accept-Language header
        var puzzleLanguage = ParseAcceptLanguage(acceptLanguage);
        
        var request = new PuzzleRequest
        {
            SizeCategory = PuzzleSizeCategory.Any,
            Language = puzzleLanguage,
            UserId = userId
        };     
        
        var puzzles = _puzzleRepositoryReader
            .GetPuzzles(sizeCategory: PuzzleSizeCategory.Any, language: puzzleLanguage)
            .Where(p => userId == null || !_userProgressRepositoryReader.IsPuzzleSolved(userId, p.Id))
            .ToList();

        if (puzzles.Count == 0)
        {
            // Should this be a 404 instead?
            throw new PuzzleNotFoundException(
                $"No puzzles found for language '{puzzleLanguage}', you probably solved all. Please try a different language.");
        }
        return Ok(puzzles[Random.Shared.Next(puzzles.Count)]);
    }

    /// <summary>
    /// Parse Accept-Language header to determine puzzle language
    /// </summary>
    private static PuzzleLanguage ParseAcceptLanguage(string? acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return PuzzleLanguage.English;
        }

        // Accept-Language format: "en-US,en;q=0.9,ru;q=0.8"
        // We'll take the first language code
        var languageCode = acceptLanguage.Split(',')[0].Split('-')[0].Trim().ToLower();

        return languageCode switch
        {
            "en" => PuzzleLanguage.English,
            "ru" => PuzzleLanguage.Russian,
            "uk" => PuzzleLanguage.Ukrainian,
            _ => PuzzleLanguage.English
        };
    }
}
