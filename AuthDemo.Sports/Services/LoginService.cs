using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AuthDemo.Sports.Services
{
    /// <summary>
    /// Service responsible for handling user login operations,
    /// including token retrieval, authentication state updates, and navigation.
    /// </summary>
    public class LoginService
    {
        private readonly ApiService _apiService;
        private readonly NavigationManager _navigationManager;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly TokenHolder _tokenHolder;
        private readonly IConfiguration _configuration;
        private readonly IApiClientFactory _apiClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginService"/> class.
        /// </summary>
        /// <param name="apiService">Service for making API requests.</param>
        /// <param name="navigationManager">Manages page navigation.</param>
        /// <param name="authenticationStateProvider">Provides and manages authentication state.</param>
        /// <param name="tokenHolder">Holds the access token and user information.</param>
        /// <param name="configuration">Application configuration for retrieving client credentials.</param>
        /// <param name="apiClientFactory">Factory to create pre-configured API clients.</param>
        public LoginService(
            ApiService apiService,
            NavigationManager navigationManager,
            AuthenticationStateProvider authenticationStateProvider,
            TokenHolder tokenHolder,
            IConfiguration configuration,
            IApiClientFactory apiClientFactory)
        {
            _apiService = apiService;
            _navigationManager = navigationManager;
            _authenticationStateProvider = authenticationStateProvider;
            _tokenHolder = tokenHolder;
            _configuration = configuration;
            _apiClientFactory = apiClientFactory;
        }

        /// <summary>
        /// Handles user login by sending credentials to the identity server
        /// and retrieving an access token if authentication is successful.
        /// </summary>
        /// <param name="model">The login model containing user email and password.</param>
        /// <returns>
        /// Returns <c>null</c> if login is successful, otherwise an error message string.
        /// </returns>
        public async Task<string?> HandleLogin(LoginModel model)
        {
            // Retrieve client credentials from configuration
            var clientId = _configuration["ClientInfo:ClientId"];
            var clientSecret = _configuration["ClientInfo:ClientSecret"];

            // Get a pre-configured API client for the identity server
            var client = _apiClientFactory.GetClient("Identity");

            // Prepare token request payload for the password grant type
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", model.Email },
                { "password", model.Password },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            // Send token request to identity server
            var response = await client.PostAsync("connect/token", new FormUrlEncodedContent(tokenRequest));

            if (response.IsSuccessStatusCode)
            {
                // Extract token from JSON response
                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();

                // Store token and user email in TokenHolder
                _tokenHolder.Token = accessToken;
                _tokenHolder.UserEmail = model.Email;

                // Update authentication state
                ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(model.Email);

                // Redirect user to the home page
                _navigationManager.NavigateTo("/");
                return null;
            }
            else
            {
                // Return error details if login fails
                var error = await response.Content.ReadAsStringAsync();
                return error ?? "An unknown error occurred.";
            }
        }
    }
}
