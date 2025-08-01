using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace AuthDemo.TransportApi.Controllers
{
    /// <summary>
    /// API controller for retrieving weather forecasts.
    /// 
    /// This controller:
    /// - Provides a list of random weather forecasts for demonstration purposes.
    /// - Requires authentication using <see cref="OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme"/>.
    /// - Enforces the <c>AdminScope</c> policy to ensure only users with the "admin.transport.api" scope can access it.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class WeatherForecastController : ControllerBase
    {
        /// <summary>
        /// Predefined weather condition summaries.
        /// </summary>
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherForecastController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance used for logging controller activities.</param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of random weather forecasts for the next 5 days.
        /// </summary>
        /// <remarks>
        /// - This endpoint generates 5 random weather forecast records.
        /// - Each forecast includes a date, temperature in Celsius, and a random weather summary.
        /// - Only users with the <c>AdminScope</c> policy (scope: admin.transport.api) can access this endpoint.
        /// </remarks>
        /// <returns>
        /// A collection of <see cref="WeatherForecast"/> objects representing the next 5 days of weather forecasts.
        /// </returns>
        [HttpGet(Name = "GetWeatherForecast")]
        [Authorize(Policy = "AdminScope")] // Requires admin.transport.api scope
        public IEnumerable<WeatherForecast> Get()
        {
            // Log all user claims for debugging purposes
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }

            // Generate and return 5 random weather forecast records
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),         // Forecast date
                TemperatureC = Random.Shared.Next(-20, 55),                        // Random temperature
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]         // Random weather summary
            })
            .ToArray();
        }
    }
}
