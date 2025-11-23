# User Authentication & Progress Tracking

## Overview

The cryptogram puzzle app uses a **lightweight anonymous authentication system** that automatically tracks solved puzzles without requiring users to sign up. This provides a seamless experience while preventing users from seeing the same puzzles repeatedly.

## Authentication Strategy

### 1. Anonymous User IDs
- **Automatic Generation**: When a user visits the site, a unique anonymous ID is automatically created
- **Format**: `anon_[UUID]` (e.g., `anon_a1b2c3d4-e5f6-7890-abcd-ef1234567890`)
- **Storage**: Saved in browser localStorage for persistence across sessions
- **No Signup Required**: Users can start playing immediately

### 2. Progress Tracking
- Solved puzzles are tracked per user ID
- Server maintains a record of which puzzles each user has completed
- Stats displayed in UI (trophy badge with count)

### 3. Smart Puzzle Selection
- "New Puzzle" button prioritizes unsolved puzzles
- Shows completion message when all puzzles in a language are solved
- Falls back gracefully if no puzzles available

## Architecture

### Backend Components

#### Models
**`UserProgress.cs`**
```csharp
public class UserProgress
{
    public string UserId { get; init; }
    public List<string> SolvedPuzzleIds { get; init; }
    public DateTime LastPlayed { get; init; }
    public int TotalPuzzlesSolved { get; init; }
}
```

**`RecordSolvedPuzzleRequest.cs`**
```csharp
public class RecordSolvedPuzzleRequest
{
    public string PuzzleId { get; init; }
    public string UserId { get; init; }
}
```

**`AvailablePuzzlesResponse.cs`**
```csharp
public class AvailablePuzzlesResponse
{
    public List<string> UnsolvedPuzzleIds { get; init; }
    public List<string> SolvedPuzzleIds { get; init; }
    public int TotalAvailable { get; init; }
    public int TotalSolved { get; init; }
}
```

#### Services
**`IUserProgressService`** - Interface for progress tracking
- `GetUserProgress(userId)` - Retrieve user's progress
- `RecordSolvedPuzzle(userId, puzzleId)` - Mark puzzle as solved
- `GetAvailablePuzzles(userId, language)` - Get unsolved puzzles
- `HasSolvedPuzzle(userId, puzzleId)` - Check if solved

**`UserProgressService`** - In-memory implementation
- Uses Dictionary<string, HashSet<string>> for fast lookups
- Thread-safe with lock mechanism
- **Note**: Currently uses in-memory storage - replace with database for production

#### API Endpoints
**`UserController`** - REST API for user operations

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/user/progress/{userId}` | GET | Get user progress |
| `/api/user/solved` | POST | Record solved puzzle |
| `/api/user/available/{userId}` | GET | Get available puzzles |
| `/api/user/has-solved/{userId}/{puzzleId}` | GET | Check if solved |

### Frontend Components

#### `auth.js`
**`UserAuth` class** - Manages authentication and progress
- `getOrCreateUserId()` - Get/create anonymous ID
- `loadProgress()` - Fetch user progress from server
- `recordSolved(puzzleId)` - Mark puzzle complete
- `getAvailablePuzzles(language)` - Get unsolved puzzles
- `hasSolved(puzzleId)` - Check if user solved puzzle
- `showCongratulations()` - Display success modal
- `updateProgressUI()` - Update stats badge

#### Integration in `script.js`
- Automatically calls `userAuth.recordSolved()` when puzzle is completed
- "New Puzzle" button checks for unsolved puzzles first
- Shows completion message when all puzzles solved

## User Experience Flow

### First Visit
1. User visits site
2. Anonymous ID automatically generated: `anon_xxx`
3. Stored in localStorage
4. Loads any available puzzle
5. Stats show "0 Solved"

### Solving a Puzzle
1. User completes puzzle successfully
2. Animated congratulations modal appears
3. Progress saved to server
4. Stats update: "1 Solved" 🏆
5. Option to try another puzzle

### Requesting New Puzzle
1. User clicks "New Puzzle"
2. System checks for unsolved puzzles in selected language
3. Loads random unsolved puzzle (preferred)
4. If all solved: Shows completion message with option to replay

### Returning Visitor
1. Same browser → localStorage retrieves user ID
2. Progress automatically loaded from server
3. Continues from where they left off
4. New browser → New anonymous ID created

## Future Enhancements

### Phase 2: Social Login (Optional)
Add social authentication to sync across devices:

```csharp
// Add to models
public class User
{
    public string Id { get; init; }
    public string? AnonymousId { get; init; } // Link to previous anonymous session
    public string? GoogleId { get; init; }
    public string? MicrosoftId { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
}
```

**Implementation**:
- Add Microsoft.AspNetCore.Authentication.Google
- Add Microsoft.AspNetCore.Authentication.MicrosoftAccount
- Merge anonymous progress when user signs in
- Sync progress across devices

### Phase 3: Database Storage
Replace in-memory storage with persistent database:

**Recommended**: Entity Framework Core with SQL Server / PostgreSQL

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<SolvedPuzzle> SolvedPuzzles { get; set; }
}

public class SolvedPuzzle
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string PuzzleId { get; set; }
    public DateTime SolvedAt { get; set; }
    public int CompletionTimeSeconds { get; set; }
}
```

### Phase 4: Advanced Features
- **Leaderboards**: Track solve times and rankings
- **Achievements**: Badges for milestones (10 solved, perfect streak, etc.)
- **Daily Challenges**: Featured puzzle of the day
- **Hints System**: Track hint usage per puzzle
- **Multiplayer**: Race against friends

## Configuration

### Current Setup (In-Memory)
```csharp
// Program.cs
builder.Services.AddSingleton<IUserProgressService, UserProgressService>();
```

### Future Setup (Database)
```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserProgressService, DatabaseUserProgressService>();
```

## Testing

### Manual Testing
1. Open browser DevTools → Application → Local Storage
2. Check for `cryptogram_user_id` entry
3. Solve a puzzle
4. Open Network tab → see POST to `/api/user/solved`
5. Refresh page → stats persist

### Unit Testing
```csharp
// Example test
[Fact]
public void RecordSolvedPuzzle_AddsToUserProgress()
{
    var service = new UserProgressService(mockCrosswordService);
    service.RecordSolvedPuzzle("user1", "puzzle1");
    
    var progress = service.GetUserProgress("user1");
    Assert.Contains("puzzle1", progress.SolvedPuzzleIds);
}
```

## Security Considerations

### Anonymous IDs
- **Pros**: No PII collected, GDPR-friendly, no passwords to secure
- **Cons**: Can't recover progress if localStorage cleared
- **Mitigation**: Offer optional account upgrade

### API Security
- Consider rate limiting for API endpoints
- Add CORS restrictions for production
- Validate user IDs on server side
- Prevent spoofing by adding IP checks (optional)

### Production Recommendations
1. **Use HTTPS** for all API calls
2. **Add authentication** for admin operations
3. **Implement CSRF protection** for state-changing operations
4. **Rate limit** puzzle solve submissions
5. **Add telemetry** to detect abuse patterns

## Benefits of This Approach

✅ **Zero Friction** - Users play immediately, no signup wall  
✅ **Privacy-Friendly** - No email or personal data required  
✅ **Progressive** - Can upgrade to social login later  
✅ **Cross-Session** - Progress persists in same browser  
✅ **Scalable** - Easy to add database later  
✅ **Simple** - Minimal authentication complexity  
✅ **Engaging** - Stats and completion tracking increase retention  

## Summary

This authentication strategy prioritizes **user experience** while still enabling progress tracking. Users can start playing immediately without creating an account, yet their progress is saved and they won't see repeat puzzles. The system is designed to be simple now and easily upgradeable to more sophisticated authentication later.
