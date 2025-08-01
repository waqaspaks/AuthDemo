using AuthDemo.Sports.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuthDemo.Shared.Models;

namespace AuthDemo.Sports.Services
{
    /// <summary>
    /// Service for retrieving game match data from the API.
    /// </summary>
    public class GameMatchService
    {
        private readonly ApiService _apiService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameMatchService"/> class.
        /// </summary>
        /// <param name="apiService">The API service used to perform HTTP requests.</param>
        public GameMatchService(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Retrieves the list of game matches from the API.
        /// </summary>
        /// <param name="accessToken">
        /// Optional access token to authenticate the request.
        /// If provided, it could be passed as an Authorization header.
        /// </param>
        /// <returns>
        /// A list of <see cref="GameMatch"/> or <c>null</c> if no matches are found.
        /// </returns>
        public async Task<List<GameMatch>?> GetMatchesAsync(string? accessToken)
        {
            // Retrieve matches using the configured ApiService client (token auto-attached if available)
            return await _apiService.GetAsync<List<GameMatch>>("Sports", "api/GameMatch");
        }
    }
}
