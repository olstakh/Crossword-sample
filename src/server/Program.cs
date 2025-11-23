using CrossWords.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register cryptogram generator and crossword service
builder.Services.AddSingleton<ICrosswordService, CrosswordService>();

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

// Serve static files from the client folder
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

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
