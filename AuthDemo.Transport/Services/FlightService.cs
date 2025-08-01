using AuthDemo.Transport.Models;
using AuthDemo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AuthDemo.Transport.Services
{
    /// <summary>
    /// Service responsible for retrieving flight schedule data from the Transport API.
    /// This service acts as a bridge between the client application and the API layer,
    /// using <see cref="ApiService"/> for HTTP communication.
    /// </summary>
    public class FlightService
    {
        private readonly ApiService _apiService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlightService"/> class.
        /// </summary>
        /// <param name="apiService">The API service used for making HTTP requests to the Transport API.</param>
        public FlightService(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Retrieves a list of all available flight schedules from the Transport API.
        /// </summary>
        /// <param name="accessToken">
        /// (Optional) The access token used for authentication. 
        /// Currently, it is not directly used in this method but can be passed for future enhancements (e.g., secure API calls).
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// The task result contains a list of <see cref="FlightSchedule"/> objects if available; otherwise, <c>null</c>.
        /// </returns>
        public async Task<List<FlightSchedule>?> GetFlightAsync(string? accessToken)
        {
            // Calls the API service to fetch all flight schedules from the "api/FlightSchedule" endpoint.
            return await _apiService.GetAsync<List<FlightSchedule>>("Transport", "api/FlightSchedule");
        }

        /// <summary>
        /// Retrieves the details of a single flight by generating a random flight ID between 1 and 50.
        /// This is mainly for demo or testing purposes.
        /// </summary>
        /// <param name="accessToken">
        /// (Optional) The access token used for authentication.
        /// Currently unused but can be integrated for secure API calls in the future.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// The task result contains a single <see cref="FlightSchedule"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public async Task<FlightSchedule?> GetFlightDetailAsync(string? accessToken)
        {
            // Generate a random flight ID between 1 and 50 for demo purposes.
            var random = new Random();
            var flight = random.Next(1, 50);

            // Calls the API service to fetch the flight schedule for the generated flight ID.
            return await _apiService.GetAsync<FlightSchedule>("Transport", $"api/FlightSchedule/{flight}");
        }
    }
}
