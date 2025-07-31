using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AuthDemo.Razor.Services
{
    public class LoginService
    {
        private readonly ApiService _apiService;
        private readonly NavigationManager _navigationManager;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly TokenHolder _tokenHolder;
        private readonly IConfiguration _configuration; 
        private readonly IApiClientFactory _apiClientFactory;

        public LoginService(ApiService apiService, NavigationManager navigationManager, AuthenticationStateProvider authenticationStateProvider, TokenHolder tokenHolder, IConfiguration configuration, IApiClientFactory apiClientFactory)
        {
            _apiService = apiService;
            _navigationManager = navigationManager;
            _authenticationStateProvider = authenticationStateProvider;
            _tokenHolder = tokenHolder;
            _configuration = configuration; 
            _apiClientFactory = apiClientFactory;
        }

        public async Task<string?> HandleLogin(LoginModel model)
        {
            var clientId = _configuration["ClientInfo:ClientId"];
            var clientSecret = _configuration["ClientInfo:ClientSecret"];
            var client = _apiClientFactory.GetClient("Identity");
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", model.Email },
                { "password", model.Password },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            var response = await client.PostAsync("connect/token", new FormUrlEncodedContent(tokenRequest));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();
                _tokenHolder.Token = accessToken;
                _tokenHolder.UserEmail = model.Email;
                ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(model.Email);
                _navigationManager.NavigateTo("/");
                return null;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return error ?? "An unknown error occurred.";
            }
        }
    }
}