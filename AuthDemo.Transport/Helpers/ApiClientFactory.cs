/// <summary>
/// Defines a factory interface for creating named <see cref="HttpClient"/> instances.
/// </summary>
/// <remarks>
/// This interface abstracts the logic of retrieving <see cref="HttpClient"/> instances
/// based on a specific API name (e.g., Identity API, Sports API, Transport API).
/// </remarks>
public interface IApiClientFactory
{
    /// <summary>
    /// Retrieves an <see cref="HttpClient"/> instance for the specified API name.
    /// </summary>
    /// <param name="apiName">The logical name of the API (e.g. "Identity", "Sports", "Transport").</param>
    /// <returns>A configured <see cref="HttpClient"/> for the requested API.</returns>
    /// <exception cref="ArgumentException">Thrown if an unknown API name is provided.</exception>
    HttpClient GetClient(string apiName);
}

/// <summary>
/// Implementation of <see cref="IApiClientFactory"/> that uses <see cref="IHttpClientFactory"/> 
/// to retrieve pre-configured <see cref="HttpClient"/> instances.
/// </summary>
/// <remarks>
/// This class ensures that each API client (Identity, Sports, Transport) is properly configured 
/// and managed through dependency injection.
/// </remarks>
public class ApiClientFactory : IApiClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="ApiClientFactory"/>.
    /// </summary>
    /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> used to create HTTP clients.</param>
    public ApiClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    #region GetClient
    /// <summary>
    /// Returns a named <see cref="HttpClient"/> based on the specified API name.
    /// </summary>
    /// <param name="apiName">The name of the API (e.g. "Identity", "Sports", "Transport").</param>
    /// <returns>The <see cref="HttpClient"/> configured for the requested API.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided API name does not match any known API.</exception>
    public HttpClient GetClient(string apiName)
    {
        return apiName switch
        {
            "Identity" => _httpClientFactory.CreateClient("AuthApi"),        // Returns client for the authentication/identity API
            "Sports" => _httpClientFactory.CreateClient("SportsApi"),      // Returns client for the sports-related API
            "Transport" => _httpClientFactory.CreateClient("TransportApi"),   // Returns client for the transport-related API
            _ => throw new ArgumentException($"Unknown API: {apiName}") // Throws an exception if API name is invalid
        };
    }
    #endregion
}
