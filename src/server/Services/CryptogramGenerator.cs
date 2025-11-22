using CrossWords.Models;

namespace CrossWords.Services;

public interface ICryptogramGenerator
{
    CrosswordPuzzle GeneratePuzzle(string puzzleId);
}

public class CryptogramGenerator : ICryptogramGenerator
{
    private readonly List<string> _quotes;

    public CryptogramGenerator()
    {
        _quotes = LoadQuotes();
    }

    private List<string> LoadQuotes()
    {
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "quotes.json");
        var json = File.ReadAllText(jsonPath);
        var options = new System.Text.Json.JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        };
        var data = System.Text.Json.JsonSerializer.Deserialize<QuotesData>(json, options);
        return data?.Quotes ?? new List<string>();
    }

    public CrosswordPuzzle GeneratePuzzle(string puzzleId)
    {
        // Use puzzle ID as seed for consistent generation
        var seed = puzzleId.GetHashCode();
        var random = new Random(seed);

        // Select a quote based on the seed
        var quoteIndex = Math.Abs(seed) % _quotes.Count;
        var quote = _quotes[quoteIndex];

        // Split quote into words and calculate grid dimensions
        var words = quote.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var (grid, numbers, letterMapping) = LayoutQuote(words, random);

        // Determine which letters to initially reveal (2-3 letters)
        var uniqueNumbers = letterMapping.Keys.OrderBy(x => x).ToList();
        var revealCount = Math.Min(3, uniqueNumbers.Count / 4); // Reveal about 25% of letters
        var initiallyRevealed = new List<int>();
        
        for (int i = 0; i < revealCount; i++)
        {
            var indexToReveal = random.Next(uniqueNumbers.Count);
            var numberToReveal = uniqueNumbers[indexToReveal];
            if (!initiallyRevealed.Contains(numberToReveal))
            {
                initiallyRevealed.Add(numberToReveal);
            }
            uniqueNumbers.RemoveAt(indexToReveal);
        }

        return new CrosswordPuzzle
        {
            Id = puzzleId,
            Title = $"Cryptogram {puzzleId}",
            Size = new PuzzleSize { Rows = grid.Count, Cols = grid[0].Count },
            Grid = grid,
            Numbers = numbers,
            LetterMapping = letterMapping,
            InitiallyRevealed = initiallyRevealed
        };
    }

    private (List<List<string>> grid, List<List<int>> numbers, Dictionary<int, string> letterMapping) 
        LayoutQuote(string[] words, Random random)
    {
        // Calculate grid dimensions - aim for roughly square-ish layout
        var totalLetters = words.Sum(w => w.Length);
        var avgWordLength = totalLetters / words.Length;
        var targetCols = Math.Max(15, (int)Math.Sqrt(totalLetters) * 2);
        
        var grid = new List<List<string>>();
        var numbers = new List<List<int>>();
        var currentRow = new List<string>();
        var currentNumberRow = new List<int>();
        var currentCol = 0;

        // Build letter to number mapping
        var letterMapping = new Dictionary<int, string>();
        var letterToNumber = new Dictionary<string, int>();
        var nextNumber = 1;

        foreach (var word in words)
        {
            // Check if word fits in current row
            if (currentCol > 0 && currentCol + word.Length > targetCols)
            {
                // Pad the rest of the row with black cells
                while (currentRow.Count < targetCols)
                {
                    currentRow.Add("#");
                    currentNumberRow.Add(0);
                }
                grid.Add(currentRow);
                numbers.Add(currentNumberRow);
                currentRow = new List<string>();
                currentNumberRow = new List<int>();
                currentCol = 0;
            }

            // Add a space (black cell) between words if not at start of row
            if (currentCol > 0)
            {
                currentRow.Add("#");
                currentNumberRow.Add(0);
                currentCol++;
            }

            // Add the word
            foreach (var letter in word)
            {
                var letterStr = letter.ToString();
                currentRow.Add(letterStr);

                // Assign number to letter
                if (!letterToNumber.ContainsKey(letterStr))
                {
                    letterToNumber[letterStr] = nextNumber;
                    letterMapping[nextNumber] = letterStr;
                    nextNumber++;
                }

                currentNumberRow.Add(letterToNumber[letterStr]);
                currentCol++;
            }
        }

        // Add the last row
        if (currentRow.Count > 0)
        {
            var finalCols = Math.Max(targetCols, currentRow.Count);
            while (currentRow.Count < finalCols)
            {
                currentRow.Add("#");
                currentNumberRow.Add(0);
            }
            grid.Add(currentRow);
            numbers.Add(currentNumberRow);
        }

        // Make sure all rows have the same length
        var maxCols = grid.Max(row => row.Count);
        foreach (var row in grid)
        {
            while (row.Count < maxCols)
            {
                row.Add("#");
            }
        }
        foreach (var row in numbers)
        {
            while (row.Count < maxCols)
            {
                row.Add(0);
            }
        }

        return (grid, numbers, letterMapping);
    }

    private class QuotesData
    {
        public List<string> Quotes { get; set; } = new();
    }
}
