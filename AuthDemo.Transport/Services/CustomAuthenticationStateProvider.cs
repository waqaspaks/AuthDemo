using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthDemo.Transport.Services
{
    /// <summary>
    /// Provides a custom implementation of <see cref="AuthenticationStateProvider"/> 
    /// to manage the authentication state in a Blazor application using JWT tokens.
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly TokenHolder _tokenHolder;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="tokenHolder">
        /// A <see cref="TokenHolder"/> instance that stores the current user's JWT token and email.
        /// </param>
        public CustomAuthenticationStateProvider(TokenHolder tokenHolder)
        {
            _tokenHolder = tokenHolder;
        }

        /// <summary>
        /// Retrieves the current authentication state based on the JWT token stored in the <see cref="TokenHolder"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AuthenticationState"/> object representing the current authentication state.
        /// </returns>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!string.IsNullOrEmpty(_tokenHolder.Token) && !string.IsNullOrEmpty(_tokenHolder.UserEmail))
            {
                // Parse the JWT token to extract claims
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(_tokenHolder.Token);

                var claims = new List<Claim>();
                claims.AddRange(token.Claims); // Add all claims from token

                // Process "scope" claim: split into individual scope claims
                var scopeClaim = claims.FirstOrDefault(c => c.Type == "scope");
                if (scopeClaim != null)
                {
                    var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var scope in scopes)
                    {
                        claims.Add(new Claim("scope", scope));
                    }
                }

                // Create identity using claims from JWT
                var identity = new ClaimsIdentity(claims, "jwt");
                _currentUser = new ClaimsPrincipal(identity);
            }
            else
            {
                // If no token is available, user is considered logged out
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        /// <summary>
        /// Marks the user as authenticated by creating a new identity using their email.
        /// </summary>
        /// <param name="email">
        /// The email of the authenticated user.
        /// </param>
        public void MarkUserAsAuthenticated(string email)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
            }, "jwt");

            _currentUser = new ClaimsPrincipal(identity);

            // Notify Blazor that the authentication state has changed
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Marks the user as logged out by clearing the token and resetting the authentication state.
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            _tokenHolder.Token = null;
            _tokenHolder.UserEmail = null;
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

            // Notify Blazor that the authentication state has changed
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
