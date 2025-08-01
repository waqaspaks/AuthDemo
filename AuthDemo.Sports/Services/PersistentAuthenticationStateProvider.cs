using AuthDemo.Sports.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace AuthDemo.Sports.Services
{
    /// <summary>
    /// Provides authentication state persistence between Blazor pages.
    /// This allows the app to restore the authenticated user from a persisted state (e.g. prerendering).
    /// </summary>
    public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
    {
        // Represents an unauthenticated user state
        private static readonly Task<AuthenticationState> _unauthenticatedTask =
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        private readonly PersistentComponentState _persistentComponentState;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentAuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="persistentComponentState">
        /// The persistent state used to store user authentication data across requests (e.g. server prerendering).
        /// </param>
        public PersistentAuthenticationStateProvider(PersistentComponentState persistentComponentState)
        {
            _persistentComponentState = persistentComponentState;
        }

        /// <summary>
        /// Retrieves the current authentication state.
        /// </summary>
        /// <returns>
        /// Returns an <see cref="AuthenticationState"/> representing either the authenticated user
        /// (if user data is persisted) or an unauthenticated state.
        /// </returns>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Try to retrieve the persisted user info from the component state
            if (!_persistentComponentState.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
            {
                // If no user info is found, return an unauthenticated state
                return _unauthenticatedTask;
            }

            // Create claims for the authenticated user
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
                new Claim(ClaimTypes.Name, userInfo.Email),
                new Claim(ClaimTypes.Email, userInfo.Email)
            };

            // Return the authentication state with a valid ClaimsPrincipal
            return Task.FromResult(
                new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity(claims, nameof(PersistentAuthenticationStateProvider)))));
        }
    }
}
