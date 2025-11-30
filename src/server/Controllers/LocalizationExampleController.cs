using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CrossWords.Resources;

namespace CrossWords.Controllers;

/// <summary>
/// Example controller demonstrating localization usage
/// You can use this pattern in any controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LocalizationExampleController : ControllerBase
{
    private readonly IStringLocalizer<SharedResources> _localizer;

    public LocalizationExampleController(IStringLocalizer<SharedResources> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Example endpoint that returns a localized greeting
    /// Test with: curl -H "Accept-Language: ru" http://localhost:5000/api/localizationexample/greeting
    /// </summary>
    [HttpGet("greeting")]
    public IActionResult GetGreeting()
    {
        // Get localized string
        var greeting = _localizer["Greeting"];
        
        return Ok(new 
        { 
            message = greeting.Value,
            culture = System.Globalization.CultureInfo.CurrentCulture.Name
        });
    }

    /// <summary>
    /// Example with parameters
    /// </summary>
    [HttpGet("welcome/{name}")]
    public IActionResult GetWelcome(string name)
    {
        // You can add parameterized strings to .resx files like:
        // <data name="WelcomeUser" xml:space="preserve">
        //   <value>Welcome, {0}!</value>
        // </data>
        
        // Then use them like:
        // var message = _localizer["WelcomeUser", name];
        
        var greeting = _localizer["Greeting"];
        
        return Ok(new 
        { 
            message = $"{greeting.Value}, {name}!"
        });
    }
}
