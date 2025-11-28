# SQLite Database Setup

## How It Works

Your user progress is now stored in a **SQLite database** (`user-progress.db`) instead of JSON.

### Database Location
```
src/server/Data/user-progress.db
```

## Why SQLite is Perfect for Your Project

✅ **Single File** - The entire database is one `.db` file  
✅ **Zero Config** - No database server to install or manage  
✅ **Survives Rebuilds** - The file stays in `Data/` folder  
✅ **Production Ready** - Can handle thousands of users  
✅ **Fast** - Indexed queries, ACID compliance  
✅ **Easy Backup** - Just copy the `.db` file  

## How Files are Organized

### Development/Building
```
src/server/
├── Data/                           # Persistent data folder
│   ├── puzzles.json               # Puzzle definitions
│   └── user-progress.db           # SQLite database ← PERSISTS HERE
├── bin/Debug/net9.0/              # Build outputs (recreated on build)
│   └── CrossWords.dll             # Your compiled code
└── obj/                           # Temporary build files
```

**Key Point:** The `Data/` folder is **NOT** inside `bin/` so it survives rebuilds!

### How the Path Works
```csharp
// In Program.cs
var userProgressDbPath = Path.Combine(
    builder.Environment.ContentRootPath,  // = "C:\...\src\server"
    "Data", 
    "user-progress.db"
);
```

This always points to: `src/server/Data/user-progress.db`

## Database Schema

```sql
CREATE TABLE UserProgress (
    UserId TEXT NOT NULL,
    PuzzleId TEXT NOT NULL,
    SolvedAt TEXT NOT NULL,
    PRIMARY KEY (UserId, PuzzleId)
);

CREATE INDEX idx_userid ON UserProgress(UserId);
```

## What Happens When You...

### Build the Project (`dotnet build`)
- ✅ Database file **stays untouched** in `Data/`
- ✅ Your data **persists**
- Only `bin/` and `obj/` folders are recreated

### Run the Server
1. App starts
2. Checks if `Data/user-progress.db` exists
3. If **doesn't exist** → Creates database + tables
4. If **exists** → Uses existing database (data preserved!)
5. Logs: `"Database stats: X users, Y total puzzles solved"`

### Solve a Puzzle
```
Client → POST /api/user/solved → SqliteUserProgressRepository
→ INSERT INTO UserProgress (UserId, PuzzleId, SolvedAt)
→ Data written to disk immediately
```

### Restart Server
- Database loads from disk
- All progress restored
- Counter shows correct counts

## Deployment

### Local Deployment
```bash
# Just copy these together:
src/server/bin/Release/net9.0/   # Compiled app
src/server/Data/                  # Database + puzzles
```

### Production Deployment
1. Publish your app:
   ```bash
   dotnet publish -c Release
   ```

2. Copy the `Data/` folder to your production server alongside binaries

3. Directory structure on server:
   ```
   /var/www/yourapp/
   ├── CrossWords.dll
   ├── CrossWords.deps.json
   ├── CrossWords.runtimeconfig.json
   ├── Data/
   │   ├── puzzles.json
   │   └── user-progress.db
   ```

## Database Files Explained

You may see these files:

- **`user-progress.db`** - Main database file (this is what you backup)
- **`user-progress.db-wal`** - Write-Ahead Log (temporary, improves performance)
- **`user-progress.db-shm`** - Shared Memory file (temporary, for concurrent access)

The `-wal` and `-shm` files are automatically managed by SQLite and deleted when the database closes cleanly.

## Inspecting the Database

### Using DB Browser for SQLite (GUI Tool)
1. Download: https://sqlitebrowser.org/
2. Open `src/server/Data/user-progress.db`
3. View/edit data in a spreadsheet-like interface

### Using SQLite CLI
```bash
sqlite3 src/server/Data/user-progress.db

# Query users
SELECT * FROM UserProgress;

# Count by user
SELECT UserId, COUNT(*) as Solved 
FROM UserProgress 
GROUP BY UserId;

# Exit
.exit
```

## Backup Strategy

### Manual Backup
```bash
# Simple copy
cp src/server/Data/user-progress.db src/server/Data/user-progress-backup.db

# With timestamp
cp src/server/Data/user-progress.db "backup-$(date +%Y%m%d).db"
```

### Automated Backup (Production)
```bash
# Cron job (daily at 2 AM)
0 2 * * * cp /var/www/yourapp/Data/user-progress.db /backups/user-progress-$(date +\%Y\%m\%d).db
```

## Migration from JSON to SQLite

If you have existing `user-progress.json`, here's a migration script:

```csharp
// Migration code (run once)
var jsonPath = "Data/user-progress.json";
if (File.Exists(jsonPath))
{
    var json = File.ReadAllText(jsonPath);
    var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
    
    var repository = app.Services.GetRequiredService<IUserProgressRepository>();
    
    foreach (var (userId, puzzleIds) in data)
    {
        foreach (var puzzleId in puzzleIds)
        {
            repository.RecordSolvedPuzzle(userId, puzzleId);
        }
    }
    
    // Rename old file so migration doesn't run again
    File.Move(jsonPath, jsonPath + ".migrated");
}
```

## Switching Back to File-Based Storage

If you prefer JSON over SQLite, just change `Program.cs`:

```csharp
// Switch to file-based
builder.Services.AddSingleton<IUserProgressRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FileUserProgressRepository>>();
    return new FileUserProgressRepository(userProgressFilePath, logger);
});
```

The interface abstraction makes switching storage easy!

## Performance Characteristics

| Operation | File (JSON) | SQLite |
|-----------|-------------|--------|
| Read single user | Load entire file | Single indexed query |
| Write puzzle | Rewrite entire file | Single INSERT |
| Check if solved | Load entire file | Indexed lookup |
| 1000 users | ~1 MB file | ~50 KB database |
| Concurrent writes | File locks | ACID transactions |

**Recommendation:** Use SQLite for production. It's faster, safer, and scales better.

## Troubleshooting

### Database locked error
```
Microsoft.Data.Sqlite.SqliteException: database is locked
```
**Solution:** Another process has the file open. Close DB Browser or other SQLite tools.

### Database not found
**Solution:** Check `ContentRootPath` - it should be `src/server`, not `src/server/bin/Debug/net9.0`

### Data disappears on rebuild
**Solution:** The database file must be in `Data/` not `bin/`. Check Program.cs path configuration.

### Slow queries
**Solution:** The table already has an index on `UserId`. For large datasets, consider adding more indexes:
```sql
CREATE INDEX idx_solved_at ON UserProgress(SolvedAt);
```

## Summary

✅ **Database location:** `src/server/Data/user-progress.db`  
✅ **Survives rebuilds:** Yes, because it's outside `bin/`  
✅ **Survives server restarts:** Yes, data persists to disk  
✅ **Deployment:** Copy `Data/` folder alongside compiled binaries  
✅ **Backup:** Just copy the `.db` file  
✅ **Migration path:** Easy to switch to PostgreSQL/SQL Server later using same interface  

Your user progress is now safely stored in a production-ready database! 🎉
