using CrossWords.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CrossWords.Services;

/// <summary>
/// SQLite-based implementation of puzzle repository
/// Stores puzzles in a single SQLite database file
/// </summary>
public class SqlitePuzzleRepository : IPuzzleRepository, IDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<SqlitePuzzleRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqlitePuzzleRepository(string dbFilePath, ILogger<SqlitePuzzleRepository> logger)
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
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
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
                CREATE TABLE IF NOT EXISTS Puzzles (
                    Id TEXT PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Language TEXT NOT NULL,
                    Rows INTEGER NOT NULL,
                    Cols INTEGER NOT NULL,
                    GridJson TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                )";
            command.ExecuteNonQuery();

            // Create indexes for faster queries
            command.CommandText = @"
                CREATE INDEX IF NOT EXISTS idx_language 
                ON Puzzles(Language)";
            command.ExecuteNonQuery();

            command.CommandText = @"
                CREATE INDEX IF NOT EXISTS idx_size 
                ON Puzzles(Rows, Cols)";
            command.ExecuteNonQuery();

            _logger.LogInformation("SQLite puzzle database initialized successfully at {ConnectionString}", _connectionString);
            
            // Log statistics
            command.CommandText = "SELECT COUNT(*) FROM Puzzles";
            var puzzleCount = Convert.ToInt32(command.ExecuteScalar() ?? 0);
            
            _logger.LogInformation("Database stats: {PuzzleCount} puzzles stored", puzzleCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing SQLite puzzle database");
            throw;
        }
    }

    public IEnumerable<CrosswordPuzzle> LoadAllPuzzles()
    {
        var puzzles = new List<CrosswordPuzzle>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Language, Rows, Cols, GridJson
                FROM Puzzles
                ORDER BY CreatedAt";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetString(0);
                var title = reader.GetString(1);
                var language = Enum.Parse<PuzzleLanguage>(reader.GetString(2));
                var rows = reader.GetInt32(3);
                var cols = reader.GetInt32(4);
                var gridJson = reader.GetString(5);

                var grid = JsonSerializer.Deserialize<List<List<string>>>(gridJson, _jsonOptions);

                if (grid != null)
                {
                    puzzles.Add(new CrosswordPuzzle
                    {
                        Id = id,
                        Title = title,
                        Language = language,
                        Size = new PuzzleSize { Rows = rows, Cols = cols },
                        Grid = grid
                    });
                }
            }

            _logger.LogInformation("Loaded {Count} puzzles from SQLite database", puzzles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading puzzles from SQLite database");
            return Enumerable.Empty<CrosswordPuzzle>();
        }

        return puzzles;
    }

    /// <summary>
    /// Add a new puzzle to the database
    /// </summary>
    public void AddPuzzle(CrosswordPuzzle puzzle)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO Puzzles (Id, Title, Language, Rows, Cols, GridJson, CreatedAt)
                VALUES ($id, $title, $language, $rows, $cols, $gridJson, $createdAt)";
            
            command.Parameters.AddWithValue("$id", puzzle.Id);
            command.Parameters.AddWithValue("$title", puzzle.Title);
            command.Parameters.AddWithValue("$language", puzzle.Language.ToString());
            command.Parameters.AddWithValue("$rows", puzzle.Size.Rows);
            command.Parameters.AddWithValue("$cols", puzzle.Size.Cols);
            command.Parameters.AddWithValue("$gridJson", JsonSerializer.Serialize(puzzle.Grid, _jsonOptions));
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));

            command.ExecuteNonQuery();
            
            _logger.LogInformation("Added puzzle {PuzzleId} ({Title}) to database", puzzle.Id, puzzle.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding puzzle {PuzzleId} to database", puzzle.Id);
            throw;
        }
    }

    /// <summary>
    /// Delete a puzzle from the database
    /// </summary>
    public void DeletePuzzle(string puzzleId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Puzzles 
                WHERE Id = $id";
            
            command.Parameters.AddWithValue("$id", puzzleId);

            var rowsAffected = command.ExecuteNonQuery();
            
            if (rowsAffected > 0)
            {
                _logger.LogInformation("Deleted puzzle {PuzzleId} from database", puzzleId);
            }
            else
            {
                _logger.LogWarning("Puzzle {PuzzleId} not found in database", puzzleId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting puzzle {PuzzleId} from database", puzzleId);
            throw;
        }
    }

    public void Dispose()
    {
        // SQLite connections are disposed when they go out of scope
        // Nothing to clean up here
    }
}
