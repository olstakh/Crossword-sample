# Localization Setup Guide

This guide explains how to add translations to the Crossword application.

## Architecture

The application uses a dual-localization approach:
1. **Server-side**: ASP.NET Core localization with `.resx` resource files
2. **Client-side**: JSON translation files loaded dynamically

## Server-Side Localization (ASP.NET Core)

### Location
Resource files are in: `src/server/Resources/`

### Files
- `SharedResources.resx` - Default/English translations
- `SharedResources.ru.resx` - Russian translations
- `SharedResources.uk.resx` - Ukrainian translations
- `SharedResources.cs` - Marker class for resource location

### How to Add Translations

1. **Open the `.resx` file** in Visual Studio (or any text editor)

2. **Add a new data entry**:
```xml
<data name="YourKey" xml:space="preserve">
  <value>Your translated text</value>
</data>
```

3. **Use in C# code**:
```csharp
using Microsoft.Extensions.Localization;

public class YourController : ControllerBase
{
    private readonly IStringLocalizer<SharedResources> _localizer;
    
    public YourController(IStringLocalizer<SharedResources> localizer)
    {
        _localizer = localizer;
    }
    
    public IActionResult SomeAction()
    {
        var translatedString = _localizer["YourKey"];
        return Ok(new { message = translatedString.Value });
    }
}
```

### Example
In `SharedResources.resx`:
```xml
<data name="WelcomeMessage" xml:space="preserve">
  <value>Welcome to Crossword Puzzles!</value>
</data>
```

In `SharedResources.ru.resx`:
```xml
<data name="WelcomeMessage" xml:space="preserve">
  <value>Добро пожаловать в кроссворды!</value>
</data>
```

In `SharedResources.uk.resx`:
```xml
<data name="WelcomeMessage" xml:space="preserve">
  <value>Ласкаво просимо до кросвордів!</value>
</data>
```

## Client-Side Localization (JavaScript)

### Location
Translation files are in: `src/client/wwwroot/locales/`

### Files
- `en.json` - English translations
- `ru.json` - Russian translations
- `uk.json` - Ukrainian translations

### How to Add Translations

1. **Add entries to JSON files**:

In `en.json`:
```json
{
  "myNewKey": "My English text"
}
```

In `ru.json`:
```json
{
  "myNewKey": "Мой русский текст"
}
```

In `uk.json`:
```json
{
  "myNewKey": "Мій український текст"
}
```

2. **Use in JavaScript**:
```javascript
// Get translation
const translatedText = localeManager.t('myNewKey', 'Fallback text');

// Use in HTML
document.getElementById('myElement').textContent = localeManager.t('myNewKey');
```

### Updating UI Text

To translate existing UI elements:

1. **Update HTML** to use data attributes or ids:
```html
<button id="checkBtn" data-i18n="checkAnswers">Check Answers</button>
```

2. **Apply translations on page load**:
```javascript
document.addEventListener('DOMContentLoaded', async () => {
    // Wait for translations to load
    await localeManager.loadTranslations();
    
    // Update all elements
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        el.textContent = localeManager.t(key);
    });
});
```

## Language Detection

The application detects language from:
1. **User selection**: Clicking flag buttons (🇬🇧 🇷🇺 🇺🇦)
2. **localStorage**: Persists user preference
3. **Accept-Language header**: Sent with all API requests

## Workflow

1. User clicks a flag → locale saved to localStorage
2. Page reloads
3. `locale.js` loads → reads locale from localStorage
4. Client loads translations from `/locales/{code}.json`
5. All API requests include `Accept-Language: en/ru/uk` header
6. Server uses header to return localized responses

## Adding a New Language

To add a new language (e.g., French):

1. **Server-side**: Create `SharedResources.fr.resx`
2. **Client-side**: Create `locales/fr.json`
3. **Update locale.js**:
```javascript
this.supportedLocales = {
    'English': { code: 'en', flag: '🇬🇧', name: 'English' },
    'Russian': { code: 'ru', flag: '🇷🇺', name: 'Русский' },
    'Ukrainian': { code: 'uk', flag: '🇺🇦', name: 'Українська' },
    'French': { code: 'fr', flag: '🇫🇷', name: 'Français' }
};
```
4. **Update Program.cs**:
```csharp
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ru"),
    new CultureInfo("uk"),
    new CultureInfo("fr")
};
```

## Best Practices

1. **Use descriptive keys**: `welcomeMessage` not `msg1`
2. **Keep fallbacks**: Always provide default text
3. **Test all languages**: Verify translations display correctly
4. **Avoid hardcoded strings**: Use translation keys everywhere
5. **Consider text length**: Translations may be longer/shorter than English

## Current Translated Strings

Client-side translations include:
- Button labels (Check Answers, Reveal Solution, etc.)
- Messages (Congratulations, Error messages)
- UI elements (Loading, Clear, etc.)

You can add more translations to the JSON files as needed!
