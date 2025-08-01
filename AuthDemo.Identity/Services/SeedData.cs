using AuthDemo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthDemo.Services;

public static class SeedData
{
    /// <summary>
    /// Seeds the client credentials (ClientId and ClientSecret) for API access.
    /// These credentials are used by client applications to authenticate and obtain tokens
    /// for accessing the secured APIs.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Register the transport_client_app
        if (await manager.FindByClientIdAsync("transport_client_app") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "transport_client_app",
                ClientSecret = "transport_client_app_secret",
                DisplayName = "Transport Client Application",
                RedirectUris = { new Uri("https://localhost:8081/callback") },
                PostLogoutRedirectUris = { new Uri("https://localhost:8081/") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.GrantTypes.Password,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + "admin.transport.api",
                    Permissions.Prefixes.Scope + "manager.transport.api",
                    Permissions.Prefixes.Scope + "user.transport.api"
                }
            });
        }

        // Register the Blazor client
        if (await manager.FindByClientIdAsync("sports_client_app") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "sports_client_app",
                ClientSecret = "sports_client_app_secret",
                DisplayName = "Sports Client Application",
                RedirectUris = { new Uri("https://localhost:7071/callback") },
                PostLogoutRedirectUris = { new Uri("https://localhost:7071/") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.GrantTypes.Password,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + "admin.sports.api",
                    Permissions.Prefixes.Scope + "manager.sports.api",
                    Permissions.Prefixes.Scope + "user.sports.api"
                }
            });
        }
    }

    /// <summary>
    /// Seeds the default users and roles into the database.
    /// These users and roles are used to define initial access levels and permissions
    /// required for authenticating and authorizing API requests.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var roles = new[] { "Admin", "Manager", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = new IdentityUser { UserName = "admin@test.com", Email = "admin@test.com" };
        if (await userManager.FindByEmailAsync(admin.Email) == null)
        {
            await userManager.CreateAsync(admin, "Admin123$");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var manager = new IdentityUser { UserName = "manager@test.com", Email = "manager@test.com" };
        if (await userManager.FindByEmailAsync(manager.Email) == null)
        {
            await userManager.CreateAsync(manager, "Manager123$");
            await userManager.AddToRoleAsync(manager, "Manager");
        }

        var user = new IdentityUser { UserName = "user@test.com", Email = "user@test.com" };
        if (await userManager.FindByEmailAsync(user.Email) == null)
        {
            await userManager.CreateAsync(user, "User123$");
            await userManager.AddToRoleAsync(user, "User");
        }
    }
}