using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace AuthDemo.SportsApi.Controllers
{
    /// <summary>
    /// Controller responsible for providing weather forecast data.
    /// This API is secured using OpenIddict token validation and requires authorized access.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class WeatherForecastController : ControllerBase
    {
        /// <summary>
        /// Predefined list of possible weather condition summaries.
        /// </summary>
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherForecastController"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for logging information and debugging.</param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of weather forecasts for the next 5 days.
        /// </summary>
        /// <remarks>
        /// - Requires an access token with the <c>AdminScope</c> policy.
        /// - Returns randomly generated weather data (for demo/testing purposes).
        /// - Each forecast includes the date, temperature in Celsius, and a summary.
        /// </remarks>
        /// <returns>
        /// A collection of <see cref="WeatherForecast"/> objects representing daily forecasts.
        /// </returns>
        /// <response code="200">Returns the list of weather forecasts successfully.</response>
        /// <response code="401">Unauthorized if the request does not contain a valid token.</response>
        [HttpGet(Name = "GetWeatherForecast")]
        [Authorize(Policy = "AdminScope")]
        public IEnumerable<WeatherForecast> Get()
        {
            // Log all claims from the authenticated user (useful for debugging token data)
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }

            // Generate weather forecast data for the next 5 days
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                // Forecast date: today + index days
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),

                // Random temperature between -20°C and 55°C
                TemperatureC = Random.Shared.Next(-20, 55),

                // Random weather condition from the predefined list
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
