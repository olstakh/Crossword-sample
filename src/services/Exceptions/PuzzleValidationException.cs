namespace CrossWords.Services.Exceptions;

public class PuzzleValidationException : Exception
{
    public PuzzleValidationException(string message) : base(message) { }

    public PuzzleValidationException(string message, Exception innerException) : base(message, innerException) { }
}
