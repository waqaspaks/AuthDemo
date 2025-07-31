using AuthDemo.Blazor.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using AuthDemo.Shared.Models;

namespace AuthDemo.Blazor.Services
{
    public class GameMatchService
    {
        private readonly ApiService _apiService;

        public GameMatchService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<GameMatch>?> GetMatchesAsync(string? accessToken)
        {
            //var headers = new Dictionary<string, string>();
            //if (!string.IsNullOrEmpty(accessToken))
            //{
            //    headers["Authorization"] = $"Bearer {accessToken}";
            //}
            return await _apiService.GetAsync<List<GameMatch>>("Mock", "api/GameMatch");
        }
    }
}
