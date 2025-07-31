using AuthDemo.Razor.Models;
using AuthDemo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AuthDemo.Razor.Services
{
    public class FlightService
    {
        private readonly ApiService _apiService;

        public FlightService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<FlightSchedule>?> GetFlightAsync(string? accessToken)
        {
            return await _apiService.GetAsync<List<FlightSchedule>>("Demo", "api/FlightSchedule");
        }

        public async Task<FlightSchedule?> GetFlightDetailAsync(string? accessToken)
        {
            var random = new Random();
            var flight = random.Next(1, 50);
            return await _apiService.GetAsync<FlightSchedule>("Demo", $"api/FlightSchedule/{flight}");
        }
    }
}
