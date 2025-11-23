namespace CrossWords.Exceptions;

public class PuzzleNotFoundException : Exception
{
    public PuzzleNotFoundException(string message) : base(message)
    {
    }

    public PuzzleNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
