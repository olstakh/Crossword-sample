using System.Runtime.CompilerServices;

public class FactRequiresServerRunningAttribute : FactAttribute
{
    private const string BaseUrl = "http://localhost:5000";

    public FactRequiresServerRunningAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        SkipUnless = nameof(IsServerRunning);
        SkipType = typeof(FactRequiresServerRunningAttribute);
        Skip = $"Test requires server running at {BaseUrl}";
    }

    public static bool IsServerRunning => ServerAvailable.Value;

    private static readonly Lazy<bool> ServerAvailable = new(() =>
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = client.GetAsync(BaseUrl).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    });
}