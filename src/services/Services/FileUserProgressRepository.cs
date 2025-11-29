using CrossWords.Services.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services;

/// <summary>
/// File-based implementation of user progress repository
/// Stores data as JSON: { "userId": ["puzzle1", "puzzle2", ...] }
/// </summary>
internal class FileUserProgressRepository : IUserProgressRepositoryReader, IUserProgressRepositoryWriter
{
    private readonly string _filePath;
    private readonly ILogger<FileUserProgressRepository> _logger;
    private readonly object _lock = new();
    private Dictionary<string, HashSet<string>> _userProgress;

    public FileUserProgressRepository(string filePath, ILogger<FileUserProgressRepository> logger)
    {
        _filePath = filePath;
        _logger = logger;
        _userProgress = new Dictionary<string, HashSet<string>>();
        
        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation("Created directory for user progress at {Directory}", directory);
        }
        
        LoadFromFile();
    }

    public bool IsPuzzleSolved(string userId, string puzzleId)
    {
        lock (_lock)
        {
            return _userProgress.ContainsKey(userId) && _userProgress[userId].Contains(puzzleId);
        }
    }

    public void RecordSolvedPuzzle(string userId, string puzzleId)
    {
        lock (_lock)
        {
            if (!_userProgress.ContainsKey(userId))
            {
                _userProgress[userId] = new HashSet<string>();
            }

            if (_userProgress[userId].Add(puzzleId))
            {
                _logger.LogInformation("User {UserId} solved puzzle {PuzzleId}", userId, puzzleId);
                SaveToFile();
            }
        }
    }

    public void ForgetPuzzles(string userId, IEnumerable<string> puzzleIds)
    {
        lock (_lock)
        {
            if (!_userProgress.ContainsKey(userId))
            {
                return;
            }

            int removedCount = 0;
            foreach (var puzzleId in puzzleIds)
            {
                if (_userProgress[userId].Remove(puzzleId))
                {
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                _logger.LogInformation("Forgot {Count} puzzle(s) for user {UserId}", removedCount, userId);
                SaveToFile();
            }
        }
    }

    public HashSet<string> GetSolvedPuzzles(string userId)
    {
        lock (_lock)
        {
            return _userProgress.ContainsKey(userId) 
                ? new HashSet<string>(_userProgress[userId]) 
                : new HashSet<string>();
        }
    }

    private void LoadFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                
                if (data != null)
                {
                    _userProgress = data.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new HashSet<string>(kvp.Value)
                    );
                    
                    var totalUsers = _userProgress.Count;
                    var totalSolved = _userProgress.Sum(kvp => kvp.Value.Count);
                    _logger.LogInformation(
                        "Loaded user progress from {FilePath}: {UserCount} users, {SolvedCount} total puzzles solved",
                        _filePath, totalUsers, totalSolved
                    );
                }
            }
            else
            {
                _logger.LogInformation("No existing user progress file found at {FilePath}, starting fresh", _filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user progress from {FilePath}", _filePath);
            _userProgress = new Dictionary<string, HashSet<string>>();
        }
    }

    private void SaveToFile()
    {
        try
        {
            // Convert HashSet to List for JSON serialization
            var data = _userProgress.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToList()
            );
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
            
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_filePath, json);
            
            _logger.LogDebug("Saved user progress to {FilePath}", _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user progress to {FilePath}", _filePath);
        }
    }
}
