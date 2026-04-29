namespace CrossWords.Services;

/// <summary>
/// Provides sanitization for user-supplied values before they are written to log entries,
/// preventing log injection/forgery attacks via newline or control characters.
/// </summary>
public static class LogSanitizer
{
    /// <summary>
    /// Sanitizes a user-supplied string for safe inclusion in log messages.
    /// Removes carriage returns and newlines to prevent log line injection,
    /// and replaces other control characters.
    /// </summary>
    public static string SanitizeForLog(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Fast path: if no dangerous characters, return as-is
        if (!ContainsUnsafeChars(value))
            return value;

        return SanitizeCore(value);
    }

    private static bool ContainsUnsafeChars(string value)
    {
        foreach (var c in value)
        {
            if (c is '\r' or '\n' or '\t' || char.IsControl(c))
                return true;
        }
        return false;
    }

    private static string SanitizeCore(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (c is '\r' or '\n')
                builder.Append('_');
            else if (char.IsControl(c))
                builder.Append('_');
            else
                builder.Append(c);
        }
        return builder.ToString();
    }
}
