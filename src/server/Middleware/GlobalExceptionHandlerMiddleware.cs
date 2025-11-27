using System.Net;
using System.Text.Json;
using CrossWords.Exceptions;

namespace CrossWords.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and returns appropriate HTTP responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode;
        object errorResponse;

        switch (exception)
        {
            case PuzzleNotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                errorResponse = new { error = notFoundEx.Message };
                break;
            
            case PuzzleValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                errorResponse = new { error = "Puzzle validation failed", details = validationEx.Message };
                break;
            
            case ArgumentNullException argNullEx:
                statusCode = HttpStatusCode.BadRequest;
                errorResponse = new { error = "Invalid request", details = argNullEx.Message };
                break;
            
            case ArgumentException argEx:
                statusCode = HttpStatusCode.BadRequest;
                errorResponse = new { error = "Invalid request", details = argEx.Message };
                break;
            
            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse = new { error = "An unexpected error occurred", details = exception.Message };
                break;
        }

        // Log based on severity
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else if (statusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
        }
        else
        {
            _logger.LogInformation(exception, "Client error: {Message}", exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, s_options));
    }
}
