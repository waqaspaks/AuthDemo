using AuthDemo.Transport.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace AuthDemo.Transport.Services
{
    /// <summary>
    /// Provides a persistent authentication state for the Blazor application.
    /// This implementation uses <see cref="PersistentComponentState"/> to store and retrieve
    /// the user's authentication information (e.g., after prerendering in Blazor Server).
    /// </summary>
    public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
    {
        /// <summary>
        /// A pre-defined unauthenticated authentication state that is returned when no user is logged in.
        /// </summary>
        private static readonly Task<AuthenticationState> _unauthenticatedTask =
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        private readonly PersistentComponentState _persistentComponentState;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentAuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="persistentComponentState">
        /// The persistent component state used to store and retrieve the <see cref="UserInfo"/> object.
        /// </param>
        public PersistentAuthenticationStateProvider(PersistentComponentState persistentComponentState)
        {
            _persistentComponentState = persistentComponentState;
        }

        /// <summary>
        /// Retrieves the current authentication state for the application.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{AuthenticationState}"/> representing the current authentication state.
        /// Returns an unauthenticated state if no user information is available.
        /// </returns>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Attempt to retrieve the user's authentication information from the persistent component state.
            if (!_persistentComponentState.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
            {
                // If no user data is found, return an unauthenticated state.
                return _unauthenticatedTask;
            }

            // Build claims based on the retrieved user information.
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.UserId), // Unique user identifier
                new Claim(ClaimTypes.Name, userInfo.Email),            // User display name
                new Claim(ClaimTypes.Email, userInfo.Email)           // User email address
            };

            // Create an authenticated user principal using the claims.
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, nameof(PersistentAuthenticationStateProvider)));

            // Return the authenticated state.
            return Task.FromResult(new AuthenticationState(authenticatedUser));
        }
    }
}
