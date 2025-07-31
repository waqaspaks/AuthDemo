using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AuthDemo.Razor.Services
{
    public class ApiService
    {
        private readonly IApiClientFactory _httpClientFactory;
        private readonly TokenHolder _tokenHolder;

        public ApiService(IApiClientFactory httpClientFactory, TokenHolder tokenHolder)
        {
            _httpClientFactory = httpClientFactory;
            _tokenHolder = tokenHolder;
        }

        private async Task<HttpClient> CreateClientAsync(string clientName)
        {
            var client = _httpClientFactory.GetClient(clientName);
            if (!string.IsNullOrEmpty(_tokenHolder.Token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenHolder.Token);
            }
            return client;
        }

        public async Task<T?> GetAsync<T>(string clientName, string uri)
        {
            var client = await CreateClientAsync(clientName);
            return await client.GetFromJsonAsync<T>(uri);
        }

        public async Task<T?> GetAsync<T>(string clientName, string uri, Dictionary<string, string>? headers)
        {
            var client = await CreateClientAsync(clientName);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (headers != null)
            {
                foreach (var kvp in headers)
                {
                    request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string clientName, string uri, T value)
        {
            var client = await CreateClientAsync(clientName);
            return await client.PostAsJsonAsync(uri, value);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string clientName, string uri, T value)
        {
            var client = await CreateClientAsync(clientName);
            return await client.PutAsJsonAsync(uri, value);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string clientName, string uri)
        {
            var client = await CreateClientAsync(clientName);
            return await client.DeleteAsync(uri);
        }
    }
}
