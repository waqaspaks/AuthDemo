using AuthDemo.Shared.Models;

namespace AuthDemo.Transport.Services
{
    /// <summary>
    /// Service class responsible for interacting with the Bus Schedule API.
    /// Provides methods to retrieve a list of bus schedules and details for a specific bus.
    /// </summary>
    public class BusService
    {
        private readonly ApiService _apiService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusService"/> class.
        /// </summary>
        /// <param name="apiService">
        /// An instance of <see cref="ApiService"/> used to make API calls to the backend.
        /// </param>
        public BusService(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Retrieves a list of all bus schedules from the backend API.
        /// </summary>
        /// <param name="accessToken">
        /// (Optional) The access token used for authorization when calling the API.
        /// This parameter is currently not used, but it can be integrated for secured API calls if required.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.  
        /// The task result contains a <see cref="List{BusSchedule}"/> with all bus schedules,
        /// or <c>null</c> if the request fails.
        /// </returns>
        public async Task<List<BusSchedule>?> GetBusAsync(string? accessToken)
        {
            // Calls the "Transport" API service and fetches the list of bus schedules
            // Endpoint: GET api/BusSchedule
            return await _apiService.GetAsync<List<BusSchedule>>("Transport", "api/BusSchedule");
        }

        /// <summary>
        /// Retrieves details of a random bus from the backend API.
        /// </summary>
        /// <param name="accessToken">
        /// (Optional) The access token used for authorization when calling the API.
        /// This parameter is currently not used, but it can be integrated for secured API calls if required.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.  
        /// The task result contains a <see cref="BusSchedule"/> object representing the details of a single bus,
        /// or <c>null</c> if the request fails.
        /// </returns>
        public async Task<BusSchedule?> GetBusDetailAsync(string? accessToken)
        {
            // Creates a random bus ID between 1 and 50 for demo purposes.
            var random = new Random();
            var bus = random.Next(1, 50);

            // Calls the "Transport" API service and fetches details of the selected bus
            // Endpoint: GET api/BusSchedule/{bus}
            return await _apiService.GetAsync<BusSchedule>("Transport", $"api/BusSchedule/{bus}");
        }
    }
}
