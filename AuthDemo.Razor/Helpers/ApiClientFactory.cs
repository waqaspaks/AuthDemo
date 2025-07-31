public interface IApiClientFactory
{
    HttpClient GetClient(string apiName);
}

public class ApiClientFactory : IApiClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public HttpClient GetClient(string apiName)
    {
        return apiName switch
        {
            "Identity" => _httpClientFactory.CreateClient("AuthApi"),
            "Mock" => _httpClientFactory.CreateClient("MockApi"),
            "Demo" => _httpClientFactory.CreateClient("DemoApi"),
            _ => throw new ArgumentException($"Unknown API: {apiName}")
        };
    }
}
