using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace AuthDemo.Blazor.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly TokenHolder _tokenHolder;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(TokenHolder tokenHolder)
        {
            _tokenHolder = tokenHolder;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!string.IsNullOrEmpty(_tokenHolder.Token) && !string.IsNullOrEmpty(_tokenHolder.UserEmail))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(_tokenHolder.Token);

                var claims = new List<Claim>();

                // Extract all claims
                claims.AddRange(token.Claims);

                // Split scope claim into multiple claims
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
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            }

            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public void MarkUserAsAuthenticated(string email)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
            }, "jwt");//"apiauth");

            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void MarkUserAsLoggedOut()
        {
            _tokenHolder.Token = null;
            _tokenHolder.UserEmail = null;
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}