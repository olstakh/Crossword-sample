using CrossWords.Services;
using CrossWords.Services.Abstractions;
using CrossWords.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

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
