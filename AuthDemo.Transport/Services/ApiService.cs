using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AuthDemo.Transport.Services
{
    /// <summary>
    /// Provides a centralized service for making HTTP requests to multiple APIs.
    /// </summary>
    /// <remarks>
    /// - Uses <see cref="IApiClientFactory"/> to retrieve named <see cref="HttpClient"/> instances.
    /// - Automatically attaches the current authentication token from <see cref="TokenHolder"/>.
    /// - Supports GET, POST, PUT, and DELETE operations.
    /// </remarks>
    public class ApiService
    {
        private readonly IApiClientFactory _httpClientFactory;
        private readonly TokenHolder _tokenHolder;

        /// <summary>
        /// Initializes a new instance of <see cref="ApiService"/>.
        /// </summary>
        /// <param name="httpClientFactory">Factory to create named API clients.</param>
        /// <param name="tokenHolder">Holds the current JWT token for authentication.</param>
        public ApiService(IApiClientFactory httpClientFactory, TokenHolder tokenHolder)
        {
            _httpClientFactory = httpClientFactory;
            _tokenHolder = tokenHolder;
        }

        #region CreateClientAsync
        /// <summary>
        /// Creates and configures an <see cref="HttpClient"/> for the specified API.
        /// </summary>
        /// <param name="clientName">The name of the API (e.g. "Identity", "Sports", "Transport").</param>
        /// <returns>An <see cref="HttpClient"/> instance with an attached bearer token if available.</returns>
        private async Task<HttpClient> CreateClientAsync(string clientName)
        {
            var client = _httpClientFactory.GetClient(clientName);

            // Attach JWT token if available
            if (!string.IsNullOrEmpty(_tokenHolder.Token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _tokenHolder.Token);
            }

            return client;
        }
        #endregion

        #region GET Methods
        /// <summary>
        /// Sends a GET request to the specified API and deserializes the response as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into.</typeparam>
        /// <param name="clientName">The name of the API client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <returns>The deserialized response or null if not found.</returns>
        public async Task<T?> GetAsync<T>(string clientName, string uri)
        {
            var client = await CreateClientAsync(clientName);
            return await client.GetFromJsonAsync<T>(uri);
        }

        /// <summary>
        /// Sends a GET request with custom headers to the specified API.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into.</typeparam>
        /// <param name="clientName">The name of the API client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <param name="headers">Optional custom headers to include in the request.</param>
        /// <returns>The deserialized response or null if not found.</returns>
        public async Task<T?> GetAsync<T>(string clientName, string uri, Dictionary<string, string>? headers)
        {
            var client = await CreateClientAsync(clientName);

            // Create a custom request to include headers
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
        #endregion

        #region POST Method
        /// <summary>
        /// Sends a POST request with a JSON body to the specified API.
        /// </summary>
        /// <typeparam name="T">The type of the request body.</typeparam>
        /// <param name="clientName">The name of the API client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <param name="value">The request payload.</param>
        /// <returns>The HTTP response message.</returns>
        public async Task<HttpResponseMessage> PostAsync<T>(string clientName, string uri, T value)
        {
            var client = await CreateClientAsync(clientName);
            return await client.PostAsJsonAsync(uri, value);
        }
        #endregion

        #region PUT Method
        /// <summary>
        /// Sends a PUT request with a JSON body to the specified API.
        /// </summary>
        /// <typeparam name="T">The type of the request body.</typeparam>
        /// <param name="clientName">The name of the API client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <param name="value">The request payload.</param>
        /// <returns>The HTTP response message.</returns>
        public async Task<HttpResponseMessage> PutAsync<T>(string clientName, string uri, T value)
        {
            var client = await CreateClientAsync(clientName);
            return await client.PutAsJsonAsync(uri, value);
        }
        #endregion

        #region DELETE Method
        /// <summary>
        /// Sends a DELETE request to the specified API.
        /// </summary>
        /// <param name="clientName">The name of the API client.</param>
        /// <param name="uri">The endpoint URI.</param>
        /// <returns>The HTTP response message.</returns>
        public async Task<HttpResponseMessage> DeleteAsync(string clientName, string uri)
        {
            var client = await CreateClientAsync(clientName);
            return await client.DeleteAsync(uri);
        }
        #endregion
    }
}
