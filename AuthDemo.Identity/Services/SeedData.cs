using AuthDemo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthDemo.Services;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Register the API client
        if (await manager.FindByClientIdAsync("client_app") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "client_app",
                ClientSecret = "client_app_secret",
                DisplayName = "API Client Application",
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
                    Permissions.Prefixes.Scope + "admin.api",
                    Permissions.Prefixes.Scope + "manager.api",
                    Permissions.Prefixes.Scope + "user.api"
                }
            });
        }

        // Register the Blazor client
        if (await manager.FindByClientIdAsync("blazor_client") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "blazor_client",
                ClientSecret = "blazor_client_secret",
                DisplayName = "Blazor Web Application",
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
                    Permissions.Prefixes.Scope + "admin.api",
                    Permissions.Prefixes.Scope + "manager.api",
                    Permissions.Prefixes.Scope + "user.api"
                }
            });
        }
    }

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