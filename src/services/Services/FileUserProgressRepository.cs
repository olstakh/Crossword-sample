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
    private Dictionary<string, Dictionary<string, DateTime>> _userProgress;

    public FileUserProgressRepository(string filePath, ILogger<FileUserProgressRepository> logger)
    {
        _filePath = filePath;
        _logger = logger;
        _userProgress = new Dictionary<string, Dictionary<string, DateTime>>();
        
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
            return _userProgress.ContainsKey(userId) && _userProgress[userId].ContainsKey(puzzleId);
        }
    }

    public void RecordSolvedPuzzle(string userId, string puzzleId)
    {
        lock (_lock)
        {
            if (!_userProgress.ContainsKey(userId))
            {
                _userProgress[userId] = new Dictionary<string, DateTime>();
            }

            if (!_userProgress[userId].ContainsKey(puzzleId))
            {
                _userProgress[userId][puzzleId] = DateTime.UtcNow;
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
                ? new HashSet<string>(_userProgress[userId].Keys) 
                : new HashSet<string>();
        }
    }

    public IEnumerable<string> GetAllUsers()
    {
        lock (_lock)
        {
            return _userProgress.Keys.ToList();
        }
    }

    public IEnumerable<UserProgressRecord> GetAllUserProgress()
    {
        lock (_lock)
        {
            var records = new List<UserProgressRecord>();
            foreach (var (userId, puzzles) in _userProgress)
            {
                foreach (var (puzzleId, solvedAt) in puzzles)
                {
                    records.Add(new UserProgressRecord
                    {
                        UserId = userId,
                        PuzzleId = puzzleId,
                        SolvedAt = solvedAt
                    });
                }
            }
            return records;
        }
    }

    public void ImportUserProgress(IEnumerable<UserProgressRecord> records)
    {
        lock (_lock)
        {
            _userProgress.Clear();
            
            foreach (var record in records)
            {
                if (!_userProgress.ContainsKey(record.UserId))
                {
                    _userProgress[record.UserId] = new Dictionary<string, DateTime>();
                }
                _userProgress[record.UserId][record.PuzzleId] = record.SolvedAt;
            }
            
            SaveToFile();
            _logger.LogInformation("Imported user progress: {Count} records", records.Count());
        }
    }

    private void LoadFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, DateTime>>>(json);
                
                if (data != null)
                {
                    _userProgress = data;
                    
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
            _userProgress = new Dictionary<string, Dictionary<string, DateTime>>();
        }
    }

    private void SaveToFile()
    {
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
            
            var json = JsonSerializer.Serialize(_userProgress, options);
            File.WriteAllText(_filePath, json);
            
            _logger.LogDebug("Saved user progress to {FilePath}", _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user progress to {FilePath}", _filePath);
        }
    }
}
