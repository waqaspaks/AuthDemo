using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AuthDemo.Transport.Services
{
    /// <summary>
    /// Service responsible for handling user authentication and login flow.
    /// It manages token retrieval, user authentication state updates, 
    /// and navigation after a successful login.
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
        /// <param name="apiService">The service used for API communication.</param>
        /// <param name="navigationManager">Manages Blazor navigation between pages.</param>
        /// <param name="authenticationStateProvider">Handles the authentication state for the application.</param>
        /// <param name="tokenHolder">Stores the user's authentication token and related info.</param>
        /// <param name="configuration">Provides access to application configuration (e.g., client credentials).</param>
        /// <param name="apiClientFactory">Factory for creating API clients for different endpoints.</param>
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
        /// Handles user login by requesting an access token from the identity server.
        /// If successful, the authentication state is updated and the user is redirected to the home page.
        /// </summary>
        /// <param name="model">
        /// The <see cref="LoginModel"/> containing the user's email and password.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// Returns <c>null</c> if the login is successful; otherwise, returns an error message.
        /// </returns>
        public async Task<string?> HandleLogin(LoginModel model)
        {
            // Retrieve the client credentials from configuration (used for token request).
            var clientId = _configuration["ClientInfo:ClientId"];
            var clientSecret = _configuration["ClientInfo:ClientSecret"];

            // Create a pre-configured HTTP client for the Identity server.
            var client = _apiClientFactory.GetClient("Identity");

            // Prepare token request payload using Resource Owner Password Credentials (ROPC) flow.
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", model.Email },
                { "password", model.Password },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };

            // Send the request to the /connect/token endpoint.
            var response = await client.PostAsync("connect/token", new FormUrlEncodedContent(tokenRequest));

            if (response.IsSuccessStatusCode)
            {
                // Parse the token response.
                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);

                // Extract the access token.
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();

                // Store token and user email for later use in API calls and authentication state.
                _tokenHolder.Token = accessToken;
                _tokenHolder.UserEmail = model.Email;

                // Mark the user as authenticated in Blazor's authentication state provider.
                ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(model.Email);

                // Redirect to the home page after successful login.
                _navigationManager.NavigateTo("/");

                return null; // No error; login successful.
            }
            else
            {
                // Read and return the error response from the server.
                var error = await response.Content.ReadAsStringAsync();
                return error ?? "An unknown error occurred.";
            }
        }
    }
}
