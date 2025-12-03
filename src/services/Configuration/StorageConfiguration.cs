namespace CrossWords.Services.Configuration;

public class StorageConfiguration
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Storage provider: "Sqlite", "InMemory", "File", "SqlServer", etc.
    /// </summary>
    public string Provider { get; set; } = "Sqlite";

    public SqliteConfiguration? Sqlite { get; set; }
    public FileConfiguration? File { get; set; }
    public SqlServerConfiguration? SqlServer { get; set; }
}

public class FileConfiguration
{
    public string PuzzlesFilePath { get; set; } = "Data/puzzles.json";
    public string UserProgressFilePath { get; set; } = "Data/user-progress.json";
}

public class SqliteConfiguration
{
    public string PuzzlesDbPath { get; set; } = "Data/puzzles.db";
    public string UserProgressDbPath { get; set; } = "Data/user-progress.db";
}

public class SqlServerConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
}
