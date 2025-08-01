using AuthDemo.Sports.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuthDemo.Shared.Models;

namespace AuthDemo.Sports.Services
{
    /// <summary>
    /// Service responsible for retrieving weather forecast data from the API.
    /// </summary>
    public class WeatherService
    {
        private readonly ApiService _apiService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherService"/> class.
        /// </summary>
        /// <param name="apiService">The API service used to perform HTTP requests.</param>
        public WeatherService(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Fetches the weather forecast from the API.
        /// </summary>
        /// <param name="accessToken">
        /// The access token used for authorization (optional, 
        /// may be handled internally by <see cref="ApiService"/>).
        /// </param>
        /// <returns>
        /// A list of <see cref="WeatherForecast"/> objects if successful, otherwise <c>null</c>.
        /// </returns>
        public async Task<List<WeatherForecast>?> GetWeatherAsync(string? accessToken)
        {
            // Call the API service to retrieve weather forecast data from the "Mock" endpoint.
            return await _apiService.GetAsync<List<WeatherForecast>>("Transport", "api/WeatherForecast");
        }
    }
}
