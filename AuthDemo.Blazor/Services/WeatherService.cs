using AuthDemo.Blazor.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuthDemo.Shared.Models;

namespace AuthDemo.Blazor.Services
{
    public class WeatherService
    {
        private readonly ApiService _apiService;

        public WeatherService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<WeatherForecast>?> GetWeatherAsync(string? accessToken)
        {
            //var headers = new Dictionary<string, string>();
            //if (!string.IsNullOrEmpty(accessToken))
            //{
            //    headers["Authorization"] = $"Bearer {accessToken}";
            //}
            return await _apiService.GetAsync<List<WeatherForecast>>("Mock", "api/WeatherForecast");
        }
    }
}
