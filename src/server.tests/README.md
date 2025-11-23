# CrossWords Tests

This test suite provides comprehensive coverage for the CrossWords puzzle application.

## Test Structure

### Unit Tests (`Services/CrosswordServiceTests.cs`)
Tests the `CrosswordService` business logic in isolation:

- **GetPuzzle_WithValidId_ReturnsPuzzle** - Verifies puzzle retrieval by ID
- **GetPuzzle_WithInvalidId_ReturnsDefaultPuzzle** - Tests fallback behavior
- **GetPuzzleBySize_ReturnsCorrectPuzzle** - Validates size-based puzzle generation
- **GetPuzzleBySize_WithSameSeed_ReturnsCachedPuzzle** - Confirms caching works
- **GetAvailablePuzzleIds_ReturnsAllPuzzles** - Lists all available puzzles
- **Puzzle_Grid_ContainsOnlyValidCharacters** - Validates grid data format
- **Puzzle_Grid_SizeMatchesMetadata** - Ensures consistency between size metadata and actual grid

### Integration Tests (`Controllers/CrosswordControllerTests.cs`)
Tests the full API endpoints with a real test server:

- **GetPuzzleList_ReturnsSuccessAndPuzzleIds** - Tests `/api/crossword/puzzles`
- **GetPuzzle_WithValidId_ReturnsSuccessAndPuzzle** - Tests `/api/crossword/puzzle/{id}`
- **GetDefaultPuzzle_ReturnsSuccess** - Tests `/api/crossword/puzzle`
- **GetPuzzleBySize_WithValidSize_ReturnsSuccess** - Tests case-insensitive enum handling
- **GetPuzzleBySize_WithInvalidSize_ReturnsBadRequest** - Validates enum validation
- **GetPuzzleBySize_WithSeed_ReturnsSuccess** - Tests seeded generation
- **GetPuzzleBySize_Small/Medium/Big_Returns...SizedGrid** - Validates size ranges
- **Puzzle_GridContainsValidData** - E2E validation of puzzle data structure

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Class
```bash
dotnet test --filter FullyQualifiedName~CrosswordServiceTests
dotnet test --filter FullyQualifiedName~CrosswordControllerTests
```

### With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Coverage

- **Service Layer**: 100% coverage of CrosswordService methods
- **API Layer**: All controller endpoints tested
- **Model Validation**: Grid structure and data format validation
- **Enum Validation**: Case-insensitive enum parsing
- **Caching**: Verified cache behavior
- **Error Handling**: Invalid inputs return appropriate status codes

## Dependencies

- **xUnit** - Test framework
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing framework
- **coverlet.collector** - Code coverage collection

## Notes

- Integration tests use the "Testing" environment to skip static file serving
- Tests are isolated and can run in parallel
- No external dependencies or database required
