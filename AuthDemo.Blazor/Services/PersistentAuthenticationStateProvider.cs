using AuthDemo.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace AuthDemo.Blazor.Services
{
    public class PersistentAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly Task<AuthenticationState> _unauthenticatedTask =
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        private readonly PersistentComponentState _persistentComponentState;

        public PersistentAuthenticationStateProvider(PersistentComponentState persistentComponentState)
        {
            _persistentComponentState = persistentComponentState;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!_persistentComponentState.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) || userInfo is null)
            {
                return _unauthenticatedTask;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
                new Claim(ClaimTypes.Name, userInfo.Email),
                new Claim(ClaimTypes.Email, userInfo.Email)
            };

            return Task.FromResult(
                new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, nameof(PersistentAuthenticationStateProvider)))));
        }
    }
}