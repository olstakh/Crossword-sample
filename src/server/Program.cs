using CrossWords.Services.Extensions;
using CrossWords.Middleware;
using CrossWords.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add localization services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configure supported cultures
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ru"),
    new CultureInfo("uk")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register crossword services based on configuration (appsettings.json)
builder.Services.AddCrosswordServices(
    builder.Configuration, 
    builder.Environment.ContentRootPath);

// Add authentication for admin endpoints
// Configure Azure AD or fallback to AdminScheme
var azureAdConfigured = !string.IsNullOrEmpty(builder.Configuration["AzureAd:TenantId"]) &&
                        !string.IsNullOrEmpty(builder.Configuration["AzureAd:ClientId"]);

if (azureAdConfigured)
{
    // Use Azure AD authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();
}
else
{
    // Fallback to custom AdminScheme
    builder.Services.AddAuthentication("AdminScheme")
        .AddScheme<AuthenticationSchemeOptions, AdminAuthHandler>("AdminScheme", null);
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        if (azureAdConfigured)
        {
            policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin"); // Users must have Admin role in Azure AD app roles
        }
        else
        {
            policy.AuthenticationSchemes.Add("AdminScheme");
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        }
    });
});

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

// Add localization middleware
app.UseRequestLocalization();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the Program class accessible for integration tests
public partial class Program { }
