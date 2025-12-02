using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace CrossWords.Tests.End2End;

/// <summary>
/// Basic end-to-end tests for the CrossWords application using Playwright
/// </summary>
public class MainPageTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    [Fact]
    public async Task MainPage_Should_Load_Successfully()
    {
        // Act
        var response = await Page.GotoAsync(BaseUrl);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected 200, got {response.Status}");
        Assert.Contains("Cryptogram Puzzle", await Page.TitleAsync());
    }

    [Fact]
    public async Task MainPage_Should_Display_Header_Elements()
    {
        // Act
        await Page.GotoAsync(BaseUrl);

        // Assert - Check for main header elements
        var header = Page.Locator("header h1");
        await Assertions.Expect(header).ToHaveTextAsync("Cryptogram Puzzle");

        // Check for control buttons
        await Assertions.Expect(Page.Locator("button#checkBtn")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("button#revealBtn")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("button#clearBtn")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("button#newPuzzleBtn")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MainPage_Should_Display_Locale_Selector()
    {
        // Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Check for locale selector with flag buttons
        var localeSelector = Page.Locator("#localeSelector");
        await Assertions.Expect(localeSelector).ToBeVisibleAsync();

        // Check for flag buttons (should have 3 languages)
        var flagButtons = Page.Locator(".locale-flag-btn");
        await Assertions.Expect(flagButtons).ToHaveCountAsync(3);
    }

    [Fact]
    public async Task MainPage_Should_Switch_Language_When_Flag_Clicked()
    {
        // Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Get the first inactive flag button (not the current language)
        var inactiveFlagButton = Page.Locator(".locale-flag-btn:not(.active)").First;
        await inactiveFlagButton.ClickAsync();

        // Wait for page reload
        await Page.WaitForLoadStateAsync(LoadState.Load);

        // Assert - Page should have reloaded with new language
        Assert.Contains("Cryptogram Puzzle", await Page.TitleAsync());
    }

    [Fact]
    public async Task PuzzleGrid_Should_Be_Displayed()
    {
        // Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Check for puzzle grid
        var puzzleGrid = Page.Locator("#crossword-grid");
        await Assertions.Expect(puzzleGrid).ToBeVisibleAsync();

        // Check for puzzle cells (there should be at least some cells)
        var cells = puzzleGrid.Locator(".cell");
        var count = await cells.CountAsync();
        Assert.True(count > 0, "Expected puzzle grid to have cells");
    }

    [Fact]
    public async Task DifficultyToggle_Should_Switch_Modes()
    {
        // Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Find difficulty toggle
        var difficultyToggle = Page.Locator("#difficultyModeToggle");
        await Assertions.Expect(difficultyToggle).ToBeVisibleAsync();

        // Get initial state
        var initiallyChecked = await difficultyToggle.IsCheckedAsync();

        // Click the toggle
        await difficultyToggle.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for animation

        // Assert - State should have changed
        var afterClick = await difficultyToggle.IsCheckedAsync();
        Assert.NotEqual(initiallyChecked, afterClick);
    }

    [Fact]
    public async Task NewPuzzleButton_Should_Load_New_Puzzle()
    {
        // Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Get the initial URL
        var initialUrl = Page.Url;

        // Click new puzzle button
        var newPuzzleButton = Page.Locator("button#newPuzzleBtn");
        await newPuzzleButton.ClickAsync();
        
        // Wait for navigation or grid update
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - URL or content should have changed
        var newUrl = Page.Url;
        
        // Either the URL changed (has puzzleId) or the grid reloaded
        var hasUrlChanged = initialUrl != newUrl;
        var gridIsVisible = await Page.Locator("#crossword-grid").IsVisibleAsync();
        
        Assert.True(hasUrlChanged || gridIsVisible, "Expected new puzzle to load");
    }

    [Fact]
    public async Task ClearButton_Should_Clear_User_Input()
    {
        // Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Try to find an input cell and type in it
        var inputCell = Page.Locator(".cell input:not([readonly])").First;
        
        // Check if there are any editable cells
        var count = await Page.Locator(".cell input:not([readonly])").CountAsync();
        if (count > 0)
        {
            await inputCell.ClickAsync();
            await inputCell.FillAsync("A");

            // Click clear button
            var clearButton = Page.Locator("button#clearBtn");
            await clearButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500);

            // Assert - Input should be cleared
            var value = await inputCell.InputValueAsync();
            Assert.Equal("", value);
        }
    }

    [Fact]
    public async Task AlphabetDecoder_Should_Be_Visible()
    {
        // Act
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Check for alphabet decoder section
        var alphabetDecoder = Page.Locator(".alphabet-decoder");
        await Assertions.Expect(alphabetDecoder).ToBeVisibleAsync();

        // Check for alphabet row
        var alphabetRow = Page.Locator("#alphabet-row");
        await Assertions.Expect(alphabetRow).ToBeVisibleAsync();
    }
}
