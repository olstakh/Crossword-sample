using CrossWords.Services.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace CrossWords.Services;

/// <summary>
/// SQLite-based implementation of user progress repository
/// Stores data in a single SQLite database file
/// </summary>
internal class SqliteUserProgressRepository : IUserProgressRepositoryReader, IUserProgressRepositoryWriter, IDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteUserProgressRepository> _logger;

    public SqliteUserProgressRepository(string dbFilePath, ILogger<SqliteUserProgressRepository> logger)
    {
        _logger = logger;
        
        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(dbFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogInformation("Created directory for SQLite database at {Directory}", directory);
        }
        
        _connectionString = $"Data Source={dbFilePath}";
        
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserProgress (
                    UserId TEXT NOT NULL,
                    PuzzleId TEXT NOT NULL,
                    SolvedAt TEXT NOT NULL,
                    PRIMARY KEY (UserId, PuzzleId)
                )";
            command.ExecuteNonQuery();

            // Create index for faster queries
            command.CommandText = @"
                CREATE INDEX IF NOT EXISTS idx_userid 
                ON UserProgress(UserId)";
            command.ExecuteNonQuery();

            _logger.LogInformation("SQLite database initialized successfully at {ConnectionString}", _connectionString);
            
            // Log statistics
            command.CommandText = "SELECT COUNT(DISTINCT UserId) FROM UserProgress";
            var userCount = Convert.ToInt32(command.ExecuteScalar() ?? 0);
            
            command.CommandText = "SELECT COUNT(*) FROM UserProgress";
            var totalSolved = Convert.ToInt32(command.ExecuteScalar() ?? 0);
            
            _logger.LogInformation("Database stats: {UserCount} users, {SolvedCount} total puzzles solved", 
                userCount, totalSolved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing SQLite database");
            throw;
        }
    }

    public bool IsPuzzleSolved(string userId, string puzzleId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) 
                FROM UserProgress 
                WHERE UserId = $userId AND PuzzleId = $puzzleId";
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$puzzleId", puzzleId);

            var count = Convert.ToInt32(command.ExecuteScalar() ?? 0);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if puzzle {PuzzleId} is solved for user {UserId}", 
                puzzleId, userId);
            return false;
        }
    }

    public void RecordSolvedPuzzle(string userId, string puzzleId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR IGNORE INTO UserProgress (UserId, PuzzleId, SolvedAt)
                VALUES ($userId, $puzzleId, $solvedAt)";
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$puzzleId", puzzleId);
            command.Parameters.AddWithValue("$solvedAt", DateTime.UtcNow.ToString("O"));

            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                _logger.LogInformation("User {UserId} solved puzzle {PuzzleId}", userId, puzzleId);
            }
            else
            {
                _logger.LogDebug("Puzzle {PuzzleId} already marked as solved for user {UserId}", 
                    puzzleId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording solved puzzle {PuzzleId} for user {UserId}", 
                puzzleId, userId);
        }
    }

    public void ForgetPuzzles(string userId, IEnumerable<string> puzzleIds)
    {
        var puzzleIdList = puzzleIds.ToList();
        if (puzzleIdList.Count == 0)
        {
            return;
        }

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            
            command.CommandText = @"
                DELETE FROM UserProgress 
                WHERE UserId = $userId AND PuzzleId = $puzzleId";
            
            var userIdParam = command.Parameters.Add("$userId", SqliteType.Text);
            var puzzleIdParam = command.Parameters.Add("$puzzleId", SqliteType.Text);
            
            userIdParam.Value = userId;
            
            int totalDeleted = 0;
            foreach (var puzzleId in puzzleIdList)
            {
                puzzleIdParam.Value = puzzleId;
                totalDeleted += command.ExecuteNonQuery();
            }
            
            transaction.Commit();
            
            _logger.LogInformation("Forgot {Count} puzzle(s) for user {UserId}", totalDeleted, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forgetting puzzles for user {UserId}", userId);
            throw;
        }
    }

    public HashSet<string> GetSolvedPuzzles(string userId)
    {
        var solvedPuzzles = new HashSet<string>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT PuzzleId 
                FROM UserProgress 
                WHERE UserId = $userId
                ORDER BY SolvedAt";
            command.Parameters.AddWithValue("$userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                solvedPuzzles.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting solved puzzles for user {UserId}", userId);
        }

        return solvedPuzzles;
    }

    public IEnumerable<string> GetAllUsers()
    {
        var users = new List<string>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT DISTINCT UserId 
                FROM UserProgress 
                ORDER BY UserId";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
        }

        return users;
    }

    public void Dispose()
    {
        // SQLite connections are disposed in using blocks
        // This method is here for future cleanup if needed
    }
}
