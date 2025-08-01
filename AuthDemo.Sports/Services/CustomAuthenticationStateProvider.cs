using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace AuthDemo.Sports.Services
{
    /// <summary>
    /// Provides a custom implementation of <see cref="AuthenticationStateProvider"/> 
    /// to manage user authentication state using JWT tokens.
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly TokenHolder _tokenHolder;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="tokenHolder">Holds the JWT token and user information.</param>
        public CustomAuthenticationStateProvider(TokenHolder tokenHolder)
        {
            _tokenHolder = tokenHolder;
        }

        /// <summary>
        /// Gets the current authentication state of the user.
        /// Reads the JWT token from <see cref="TokenHolder"/> and extracts claims.
        /// </summary>
        /// <returns>The current <see cref="AuthenticationState"/>.</returns>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!string.IsNullOrEmpty(_tokenHolder.Token) && !string.IsNullOrEmpty(_tokenHolder.UserEmail))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(_tokenHolder.Token);

                var claims = new List<Claim>();

                // Extract all claims from the token
                claims.AddRange(token.Claims);

                // Split the "scope" claim into multiple individual claims if it exists
                var scopeClaim = claims.FirstOrDefault(c => c.Type == "scope");
                if (scopeClaim != null)
                {
                    var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var scope in scopes)
                    {
                        claims.Add(new Claim("scope", scope));
                    }
                }

                var identity = new ClaimsIdentity(claims, "jwt");
                _currentUser = new ClaimsPrincipal(identity);
            }
            else
            {
                // No valid token, user is anonymous
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        /// <summary>
        /// Marks the user as authenticated and updates the authentication state.
        /// This does NOT validate the token, only sets the identity for the UI.
        /// </summary>
        /// <param name="email">The email of the authenticated user.</param>
        public void MarkUserAsAuthenticated(string email)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
            }, "jwt");

            _currentUser = new ClaimsPrincipal(identity);

            // Notify Blazor components that the authentication state has changed
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Logs out the user by clearing the token and resetting the authentication state.
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            _tokenHolder.Token = null;
            _tokenHolder.UserEmail = null;
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

            // Notify Blazor components that the authentication state has changed
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
