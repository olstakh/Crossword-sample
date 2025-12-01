using CrossWords.Services.Models;
using CrossWords.Services.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services;

internal class FilePuzzleRepository : IPuzzleRepositoryReader, IPuzzleRepositoryWriter
{
    private readonly string _puzzlesFilePath;
    private readonly ILogger<FilePuzzleRepository> _logger;
    private readonly object _lock = new();
    private List<CrosswordPuzzle> _puzzles;

    public FilePuzzleRepository(string puzzlesFilePath, ILogger<FilePuzzleRepository> logger)
    {
        _puzzlesFilePath = puzzlesFilePath;
        _logger = logger;
        _puzzles = new List<CrosswordPuzzle>();
        
        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(_puzzlesFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation("Created directory for puzzles at {Directory}", directory);
        }
        
        LoadFromFile();
    }

    private void LoadFromFile()
    {
        lock (_lock)
        {
            try
            {
                if (File.Exists(_puzzlesFilePath))
                {
                    var jsonContent = File.ReadAllText(_puzzlesFilePath);
                    var puzzleList = JsonSerializer.Deserialize<List<CrosswordPuzzle>>(jsonContent);
                    _puzzles = puzzleList ?? new List<CrosswordPuzzle>();
                    _logger.LogInformation("Loaded {Count} puzzles from {FilePath}", _puzzles.Count, _puzzlesFilePath);
                }
                else
                {
                    _logger.LogWarning("Puzzles file not found at {FilePath}, starting with empty collection", _puzzlesFilePath);
                    _puzzles = new List<CrosswordPuzzle>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading puzzles from file: {FilePath}", _puzzlesFilePath);
                _puzzles = new List<CrosswordPuzzle>();
            }
        }
    }

    private void SaveToFile()
    {
        lock (_lock)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(_puzzles, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(_puzzlesFilePath, jsonContent);
                _logger.LogInformation("Saved {Count} puzzles to {FilePath}", _puzzles.Count, _puzzlesFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving puzzles to file: {FilePath}", _puzzlesFilePath);
            }
        }
    }

    public IEnumerable<CrosswordPuzzle> LoadAllPuzzles()
    {
        lock (_lock)
        {
            return _puzzles.ToList();
        }
    }

    public IEnumerable<CrosswordPuzzle> GetPuzzles(PuzzleSizeCategory sizeCategory = PuzzleSizeCategory.Any, PuzzleLanguage? language = null)
    {
        var allPuzzles = LoadAllPuzzles();

        // Filter by language if specified
        if (language.HasValue)
        {
            allPuzzles = allPuzzles.Where(p => p.Language == language.Value);
        }

        // Filter by size category if not Any
        if (sizeCategory != PuzzleSizeCategory.Any)
        {
            var (minSize, maxSize) = sizeCategory.GetSizeRange();
            allPuzzles = allPuzzles.Where(p => 
                p.Size.Rows >= minSize && p.Size.Rows <= maxSize &&
                p.Size.Cols >= minSize && p.Size.Cols <= maxSize);
        }

        return allPuzzles;
    }

    public CrosswordPuzzle? GetPuzzle(string puzzleId)
    {
        lock (_lock)
        {
            return _puzzles.FirstOrDefault(p => p.Id == puzzleId);
        }
    }

    public void AddPuzzle(CrosswordPuzzle puzzle)
    {
        lock (_lock)
        {
            var existingIndex = _puzzles.FindIndex(p => p.Id == puzzle.Id);
            if (existingIndex >= 0)
            {
                _puzzles[existingIndex] = puzzle;
                _logger.LogInformation("Updated puzzle {PuzzleId}", puzzle.Id);
            }
            else
            {
                _puzzles.Add(puzzle);
                _logger.LogInformation("Added new puzzle {PuzzleId}", puzzle.Id);
            }
            SaveToFile();
        }
    }

    public void DeletePuzzle(string puzzleId)
    {
        lock (_lock)
        {
            var removed = _puzzles.RemoveAll(p => p.Id == puzzleId);
            if (removed > 0)
            {
                _logger.LogInformation("Deleted puzzle {PuzzleId}", puzzleId);
                SaveToFile();
            }
            else
            {
                _logger.LogWarning("Attempted to delete non-existent puzzle {PuzzleId}", puzzleId);
            }
        }
    }

    public void AddPuzzles(IEnumerable<CrosswordPuzzle> puzzles)
    {
        lock (_lock)
        {
            foreach (var puzzle in puzzles)
            {
                var existingIndex = _puzzles.FindIndex(p => p.Id == puzzle.Id);
                if (existingIndex >= 0)
                {
                    _puzzles[existingIndex] = puzzle;
                }
                else
                {
                    _puzzles.Add(puzzle);
                }
            }
            _logger.LogInformation("Added/updated {Count} puzzles", puzzles.Count());
            SaveToFile();
        }
    }
}
