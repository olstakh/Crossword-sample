namespace CrossWords.Services.Configuration;

public class StorageConfiguration
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Storage provider: "Sqlite", "InMemory", "SqlServer", etc.
    /// </summary>
    public string Provider { get; set; } = "Sqlite";

    public SqliteConfiguration? Sqlite { get; set; }
    public SqlServerConfiguration? SqlServer { get; set; }
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
