using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AuthDemo.Sports.Services
{
    /// <summary>
    /// Provides a wrapper around HttpClient for making authenticated API calls (GET, POST, PUT, DELETE).
    /// Automatically attaches the Bearer token from <see cref="TokenHolder"/> if available.
    /// </summary>
    public class ApiService
    {
        private readonly IApiClientFactory _httpClientFactory;
        private readonly TokenHolder _tokenHolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Factory for creating named <see cref="HttpClient"/> instances.</param>
        /// <param name="tokenHolder">Holds the current authentication token.</param>
        public ApiService(IApiClientFactory httpClientFactory, TokenHolder tokenHolder)
        {
            _httpClientFactory = httpClientFactory;
            _tokenHolder = tokenHolder;
        }

        /// <summary>
        /// Creates an <see cref="HttpClient"/> for the specified client name and attaches the Bearer token if available.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
        private async Task<HttpClient> CreateClientAsync(string clientName)
        {
            var client = _httpClientFactory.GetClient(clientName);

            // Attach Bearer token if available
            if (!string.IsNullOrEmpty(_tokenHolder.Token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _tokenHolder.Token);
            }

            return client;
        }

        /// <summary>
        /// Sends a GET request and deserializes the response to the specified type.
        /// </summary>
        /// <typeparam name="T">The expected response type.</typeparam>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <returns>The deserialized response or <c>null</c> if not found.</returns>
        public async Task<T?> GetAsync<T>(string clientName, string uri)
        {
            var client = await CreateClientAsync(clientName);
            return await client.GetFromJsonAsync<T>(uri);
        }

        /// <summary>
        /// Sends a GET request with custom headers and deserializes the response to the specified type.
        /// </summary>
        /// <typeparam name="T">The expected response type.</typeparam>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <param name="headers">Optional custom headers to include in the request.</param>
        /// <returns>The deserialized response or <c>null</c> if not found.</returns>
        public async Task<T?> GetAsync<T>(string clientName, string uri, Dictionary<string, string>? headers)
        {
            var client = await CreateClientAsync(clientName);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Add custom headers if provided
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

        /// <summary>
        /// Sends a POST request with a JSON payload.
        /// </summary>
        /// <typeparam name="T">The type of the request payload.</typeparam>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <param name="value">The request body to send.</param>
        /// <returns>The <see cref="HttpResponseMessage"/> from the API.</returns>
        public async Task<HttpResponseMessage> PostAsync<T>(string clientName, string uri, T value)
        {
            var client = await CreateClientAsync(clientName);
            return await client.PostAsJsonAsync(uri, value);
        }

        /// <summary>
        /// Sends a PUT request with a JSON payload.
        /// </summary>
        /// <typeparam name="T">The type of the request payload.</typeparam>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <param name="value">The request body to update.</param>
        /// <returns>The <see cref="HttpResponseMessage"/> from the API.</returns>
        public async Task<HttpResponseMessage> PutAsync<T>(string clientName, string uri, T value)
        {
            var client = await CreateClientAsync(clientName);
            return await client.PutAsJsonAsync(uri, value);
        }

        /// <summary>
        /// Sends a DELETE request to the specified endpoint.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <returns>The <see cref="HttpResponseMessage"/> from the API.</returns>
        public async Task<HttpResponseMessage> DeleteAsync(string clientName, string uri)
        {
            var client = await CreateClientAsync(clientName);
            return await client.DeleteAsync(uri);
        }
    }
}
