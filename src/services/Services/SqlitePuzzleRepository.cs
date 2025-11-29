using CrossWords.Services.Models;
using CrossWords.Services.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CrossWords.Services;

/// <summary>
/// SQLite-based implementation of puzzle repository
/// Stores puzzles in a single SQLite database file
/// </summary>
internal class SqlitePuzzleRepository : IPuzzleRepositoryReader, IPuzzleRepositoryWriter, IDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<SqlitePuzzleRepository> _logger;
    private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

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
                    RevealedLettersJson TEXT,
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
                SELECT Id, Title, Language, Rows, Cols, GridJson, RevealedLettersJson
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
                var revealedLettersJson = reader.IsDBNull(6) ? null : reader.GetString(6);

                var grid = JsonSerializer.Deserialize<List<List<string>>>(gridJson, s_jsonOptions);
                var revealedLetters = string.IsNullOrEmpty(revealedLettersJson) 
                    ? null 
                    : JsonSerializer.Deserialize<List<string>>(revealedLettersJson, s_jsonOptions);

                if (grid != null)
                {
                    puzzles.Add(new CrosswordPuzzle
                    {
                        Id = id,
                        Title = title,
                        Language = language,
                        Size = new PuzzleSize { Rows = rows, Cols = cols },
                        Grid = grid,
                        RevealedLetters = revealedLetters
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

    public IEnumerable<CrosswordPuzzle> GetPuzzles(PuzzleSizeCategory sizeCategory = PuzzleSizeCategory.Any, PuzzleLanguage? language = null)
    {
        var puzzles = new List<CrosswordPuzzle>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            
            // Build query with filters
            var whereClauses = new List<string>();
            
            if (language.HasValue)
            {
                whereClauses.Add("Language = $language");
            }
            
            if (sizeCategory != PuzzleSizeCategory.Any)
            {
                var (minSize, maxSize) = sizeCategory.GetSizeRange();
                whereClauses.Add("(Rows BETWEEN $minSize AND $maxSize)");
                whereClauses.Add("(Cols BETWEEN $minSize AND $maxSize)");
            }
            
            var whereClause = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";
            
            command.CommandText = $@"
                SELECT Id, Title, Language, Rows, Cols, GridJson, RevealedLettersJson
                FROM Puzzles
                {whereClause}
                ORDER BY CreatedAt";
            
            if (language.HasValue)
            {
                command.Parameters.AddWithValue("$language", language.Value.ToString());
            }
            
            if (sizeCategory != PuzzleSizeCategory.Any)
            {
                var (minSize, maxSize) = sizeCategory.GetSizeRange();
                command.Parameters.AddWithValue("$minSize", minSize);
                command.Parameters.AddWithValue("$maxSize", maxSize);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetString(0);
                var title = reader.GetString(1);
                var languageValue = Enum.Parse<PuzzleLanguage>(reader.GetString(2));
                var rows = reader.GetInt32(3);
                var cols = reader.GetInt32(4);
                var gridJson = reader.GetString(5);
                var revealedLettersJson = reader.IsDBNull(6) ? null : reader.GetString(6);

                var grid = JsonSerializer.Deserialize<List<List<string>>>(gridJson, s_jsonOptions);
                var revealedLetters = string.IsNullOrEmpty(revealedLettersJson) 
                    ? null 
                    : JsonSerializer.Deserialize<List<string>>(revealedLettersJson, s_jsonOptions);

                if (grid != null)
                {
                    puzzles.Add(new CrosswordPuzzle
                    {
                        Id = id,
                        Title = title,
                        Language = languageValue,
                        Size = new PuzzleSize { Rows = rows, Cols = cols },
                        Grid = grid,
                        RevealedLetters = revealedLetters
                    });
                }
            }

            _logger.LogInformation("Loaded {Count} puzzles with filters: size={Size}, language={Language}", 
                puzzles.Count, sizeCategory, language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading filtered puzzles from SQLite database");
            return Enumerable.Empty<CrosswordPuzzle>();
        }

        return puzzles;
    }

    public CrosswordPuzzle? GetPuzzle(string puzzleId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Title, Language, Rows, Cols, GridJson, RevealedLettersJson
                FROM Puzzles
                WHERE Id = $id";
            
            command.Parameters.AddWithValue("$id", puzzleId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var id = reader.GetString(0);
                var title = reader.GetString(1);
                var language = Enum.Parse<PuzzleLanguage>(reader.GetString(2));
                var rows = reader.GetInt32(3);
                var cols = reader.GetInt32(4);
                var gridJson = reader.GetString(5);
                var revealedLettersJson = reader.IsDBNull(6) ? null : reader.GetString(6);

                var grid = JsonSerializer.Deserialize<List<List<string>>>(gridJson, s_jsonOptions);
                var revealedLetters = string.IsNullOrEmpty(revealedLettersJson) 
                    ? null 
                    : JsonSerializer.Deserialize<List<string>>(revealedLettersJson, s_jsonOptions);

                if (grid != null)
                {
                    return new CrosswordPuzzle
                    {
                        Id = id,
                        Title = title,
                        Language = language,
                        Size = new PuzzleSize { Rows = rows, Cols = cols },
                        Grid = grid,
                        RevealedLetters = revealedLetters
                    };
                }
            }

            _logger.LogInformation("Puzzle {PuzzleId} not found", puzzleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading puzzle {PuzzleId} from SQLite database", puzzleId);
        }

        return null;
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
                INSERT OR REPLACE INTO Puzzles (Id, Title, Language, Rows, Cols, GridJson, RevealedLettersJson, CreatedAt)
                VALUES ($id, $title, $language, $rows, $cols, $gridJson, $revealedLettersJson, $createdAt)";
            
            command.Parameters.AddWithValue("$id", puzzle.Id);
            command.Parameters.AddWithValue("$title", puzzle.Title);
            command.Parameters.AddWithValue("$language", puzzle.Language.ToString());
            command.Parameters.AddWithValue("$rows", puzzle.Size.Rows);
            command.Parameters.AddWithValue("$cols", puzzle.Size.Cols);
            command.Parameters.AddWithValue("$gridJson", JsonSerializer.Serialize(puzzle.Grid, s_jsonOptions));
            
            var revealedLettersJson = puzzle.RevealedLetters != null && puzzle.RevealedLetters.Count > 0
                ? JsonSerializer.Serialize(puzzle.RevealedLetters, s_jsonOptions)
                : (object)DBNull.Value;
            command.Parameters.AddWithValue("$revealedLettersJson", revealedLettersJson);
            
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
    /// Add multiple puzzles to the database in bulk using a transaction
    /// </summary>
    public void AddPuzzles(IEnumerable<CrosswordPuzzle> puzzles)
    {
        var puzzleList = puzzles.ToList();
        if (!puzzleList.Any())
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
                INSERT OR REPLACE INTO Puzzles (Id, Title, Language, Rows, Cols, GridJson, CreatedAt)
                VALUES ($id, $title, $language, $rows, $cols, $gridJson, $createdAt)";
            
            var idParam = command.Parameters.Add("$id", SqliteType.Text);
            var titleParam = command.Parameters.Add("$title", SqliteType.Text);
            var languageParam = command.Parameters.Add("$language", SqliteType.Text);
            var rowsParam = command.Parameters.Add("$rows", SqliteType.Integer);
            var colsParam = command.Parameters.Add("$cols", SqliteType.Integer);
            var gridJsonParam = command.Parameters.Add("$gridJson", SqliteType.Text);
            var createdAtParam = command.Parameters.Add("$createdAt", SqliteType.Text);
            
            var createdAt = DateTime.UtcNow.ToString("O");

            foreach (var puzzle in puzzleList)
            {
                idParam.Value = puzzle.Id;
                titleParam.Value = puzzle.Title;
                languageParam.Value = puzzle.Language.ToString();
                rowsParam.Value = puzzle.Size.Rows;
                colsParam.Value = puzzle.Size.Cols;
                gridJsonParam.Value = JsonSerializer.Serialize(puzzle.Grid, s_jsonOptions);
                createdAtParam.Value = createdAt;

                command.ExecuteNonQuery();
            }

            transaction.Commit();
            
            _logger.LogInformation("Added {Count} puzzles to database in bulk", puzzleList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding {Count} puzzles to database in bulk", puzzleList.Count);
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
