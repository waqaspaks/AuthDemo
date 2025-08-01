/// <summary>
/// Defines a contract for creating and retrieving pre-configured HttpClient instances 
/// for different APIs based on a given name.
/// </summary>
public interface IApiClientFactory
{
    /// <summary>
    /// Retrieves a configured HttpClient instance for the specified API.
    /// </summary>
    /// <param name="apiName">The name of the API (e.g., "Identity", "Sports", "Transport").</param>
    /// <returns>A configured <see cref="HttpClient"/> for the specified API.</returns>
    HttpClient GetClient(string apiName);
}

/// <summary>
/// A factory implementation for retrieving named HttpClient instances 
/// using the built-in IHttpClientFactory.
/// </summary>
public class ApiClientFactory : IApiClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="ApiClientFactory"/>.
    /// </summary>
    /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> used to create HttpClient instances.</param>
    public ApiClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory
            ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    /// <summary>
    /// Retrieves a configured HttpClient instance based on the provided API name.
    /// </summary>
    /// <param name="apiName">The name of the API (e.g., "Identity", "Sports", "Transport").</param>
    /// <returns>A configured <see cref="HttpClient"/> for the requested API.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided API name is not recognized.</exception>
    public HttpClient GetClient(string apiName)
    {
        return apiName switch
        {
            "Identity" => _httpClientFactory.CreateClient("AuthApi"),       // Returns client for the authentication/identity API
            "Sports" => _httpClientFactory.CreateClient("SportsApi"),     // Returns client for the sports-related API
            "Transport" => _httpClientFactory.CreateClient("TransportApi"), // Returns client for the transport-related API
            _ => throw new ArgumentException($"Unknown API: {apiName}")    // Throws an error if the API name is invalid
        };
    }
}
