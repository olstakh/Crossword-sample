using CrossWords.Models;
using CrossWords.Services;
using System.Text.Json;

namespace CrossWords.Utilities;

/// <summary>
/// Utility to migrate puzzles from JSON file to SQLite database
/// </summary>
public static class PuzzleMigration
{
    /// <summary>
    /// Migrate puzzles from JSON file to SQLite database
    /// </summary>
    public static void MigrateFromJsonToSqlite(string jsonFilePath, string dbFilePath, ILogger logger)
    {
        try
        {
            // Check if JSON file exists
            if (!File.Exists(jsonFilePath))
            {
                logger.LogWarning("JSON puzzle file not found at {Path}. Skipping migration.", jsonFilePath);
                return;
            }

            // Check if database already has puzzles
            using (var testRepo = new SqlitePuzzleRepository(dbFilePath, 
                LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SqlitePuzzleRepository>()))
            {
                var existingPuzzles = testRepo.LoadAllPuzzles().ToList();
                if (existingPuzzles.Any())
                {
                    logger.LogInformation("Database already contains {Count} puzzles. Skipping migration.", existingPuzzles.Count);
                    return;
                }
            }

            // Load puzzles from JSON
            logger.LogInformation("Loading puzzles from {Path}", jsonFilePath);
            var jsonContent = File.ReadAllText(jsonFilePath);
            var puzzles = JsonSerializer.Deserialize<List<CrosswordPuzzle>>(jsonContent);

            if (puzzles == null || !puzzles.Any())
            {
                logger.LogWarning("No puzzles found in JSON file. Skipping migration.");
                return;
            }

            logger.LogInformation("Found {Count} puzzles in JSON file. Starting migration...", puzzles.Count);

            // Create SQLite repository and add all puzzles
            var sqliteLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SqlitePuzzleRepository>();
            using var sqliteRepo = new SqlitePuzzleRepository(dbFilePath, sqliteLogger);

            var migratedCount = 0;
            foreach (var puzzle in puzzles)
            {
                try
                {
                    sqliteRepo.AddPuzzle(puzzle);
                    migratedCount++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error migrating puzzle {PuzzleId}", puzzle.Id);
                }
            }

            logger.LogInformation("Migration completed. Successfully migrated {Count} out of {Total} puzzles.", 
                migratedCount, puzzles.Count);

            // Optional: Rename the JSON file to indicate it's been migrated
            var backupPath = jsonFilePath + ".migrated";
            if (!File.Exists(backupPath))
            {
                File.Move(jsonFilePath, backupPath);
                logger.LogInformation("Backed up original JSON file to {Path}", backupPath);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during puzzle migration");
            throw;
        }
    }
}
