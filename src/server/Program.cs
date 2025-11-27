using CrossWords.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register cryptogram generator and crossword service
var puzzlesDbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "puzzles.db");
builder.Services.AddSingleton<SqlitePuzzleRepository>(sp => 
{
    var logger = sp.GetRequiredService<ILogger<SqlitePuzzleRepository>>();
    return new SqlitePuzzleRepository(puzzlesDbPath, logger);
});
builder.Services.AddSingleton<IPuzzleRepository>(sp => sp.GetRequiredService<SqlitePuzzleRepository>());
builder.Services.AddSingleton<IPuzzleRepositoryPersister>(sp => sp.GetRequiredService<SqlitePuzzleRepository>());
builder.Services.AddSingleton<ICrosswordService, CrosswordService>();

// Register user progress repository and service
var userProgressDbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "user-progress.db");
builder.Services.AddSingleton<IUserProgressRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<SqliteUserProgressRepository>>();
    return new SqliteUserProgressRepository(userProgressDbPath, logger);
});
builder.Services.AddSingleton<IUserProgressService, UserProgressService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Migrate existing puzzles from JSON to SQLite if needed
var puzzlesJsonPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "puzzles.json");
CrossWords.Utilities.PuzzleMigration.MigrateFromJsonToSqlite(
    puzzlesJsonPath, 
    puzzlesDbPath, 
    app.Logger);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files from the client folder (skip in test environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    var clientPath = Path.Combine(Directory.GetCurrentDirectory(), "client", "wwwroot");
    // Fallback for local development
    if (!Directory.Exists(clientPath))
    {
        clientPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "client", "wwwroot");
    }

    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(clientPath),
        RequestPath = ""
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(clientPath),
        RequestPath = ""
    });
}

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the Program class accessible for integration tests
public partial class Program { }
