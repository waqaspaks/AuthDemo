using AuthDemo.Sports.Components;
using AuthDemo.Sports.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

//
// ==========================================================
// Program.cs - Entry point for AuthDemo.Sports (Blazor Server App)
//
// SUMMARY:
// This application is a Blazor Server project that implements authentication 
// and authorization using cookies and custom policies based on both roles and scopes.
// It connects to multiple APIs (Auth API, Demo API, Mock API) using HttpClient factories.
//
// KEY FEATURES:
// - Cookie-based authentication with configurable login and access-denied paths.
// - Custom AuthenticationStateProvider for Blazor authentication state tracking.
// - Role-based and scope-based authorization policies.
// - Dependency injection for authentication services and API clients.
// - Configurable API endpoints via appsettings.
// - Logout endpoint for session termination.
// ==========================================================
//

var builder = WebApplication.CreateBuilder(args);

#region Service Configuration

// Add Razor components (Blazor server-side components)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register authentication state provider for Blazor components
// This makes AuthenticationState available across the application.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// TokenHolder is a singleton service for storing authentication tokens (e.g. JWT)
builder.Services.AddSingleton<TokenHolder>();

// ----------------------------------------------------------
// AUTHENTICATION CONFIGURATION
// ----------------------------------------------------------
// Cookie authentication is configured here. Users will be authenticated using cookies.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        var config = builder.Configuration;

        // Login redirection path
        options.LoginPath = config["Auth:LoginPath"] ?? "/login";

        // Access denied redirection path
        options.AccessDeniedPath = config["Auth:AccessDeniedPath"] ?? "/accessdenied";

        // Cookie expiration time (default 30 minutes if not configured in appsettings)
        options.ExpireTimeSpan = TimeSpan.FromMinutes(int.TryParse(config["Auth:ExpireMinutes"], out var min) ? min : 30);

        // Enables sliding expiration (extends session expiration on activity)
        options.SlidingExpiration = true;
    });

// ----------------------------------------------------------
// AUTHORIZATION CONFIGURATION
// ----------------------------------------------------------
// Role-based policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    // Additional role-based policies can be added here
});

// ----------------------------------------------------------
// REGISTER APPLICATION SERVICES
// ----------------------------------------------------------
// Services responsible for authentication, API calls, and domain logic.
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<RegisterService>();
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<GameMatchService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<IApiClientFactory, ApiClientFactory>();

// ----------------------------------------------------------
// HTTP CLIENT CONFIGURATION
// ----------------------------------------------------------
// Configure multiple HttpClient instances for different APIs
// Base URLs are taken from appsettings.json (with fallbacks)
var transportApiBaseUrl = builder.Configuration["Api:transportApiBaseUrl"] ?? "https://localhost:7389";
var sportsApiBaseUrl = builder.Configuration["Api:sportsApiBaseUrl"] ?? "https://localhost:7236";
var authApiBaseUrl = builder.Configuration["Api:AuthApiBaseUrl"] ?? "https://localhost:7214";

// HttpClient for Authentication API
builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri(authApiBaseUrl);
});

// HttpClient for Transport API
builder.Services.AddHttpClient("TransportApi", client =>
{
    client.BaseAddress = new Uri(transportApiBaseUrl);
});

// HttpClient for Sports API
builder.Services.AddHttpClient("SportsApi", client =>
{
    client.BaseAddress = new Uri(sportsApiBaseUrl);
});

// ----------------------------------------------------------
// SCOPE-BASED AUTHORIZATION POLICIES
// ----------------------------------------------------------
// These policies are evaluated using claims instead of roles.
// Policies verify if a user has a specific "scope" claim.
builder.Services.AddAuthorization(options =>
{
    // Admin policy checks if the user has admin scope in transport or sports API
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "admin.transport.api") ||
            context.User.HasClaim("scope", "admin.sports.api")
        );
    });

    // Manager policy checks if the user has manager scope in transport or sports API
    options.AddPolicy("ManagerPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "manager.transport.api") ||
            context.User.HasClaim("scope", "manager.sports.api")
        );
    });

    // User policy checks if the user has sports API user scope
    options.AddPolicy("UserPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "user.sports.api")
        );
    });
});

#endregion

var app = builder.Build();

#region Middleware Pipeline Configuration

// ----------------------------------------------------------
// ERROR HANDLING
// ----------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    // Production error handling
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // Enable HSTS (forces HTTPS)
    app.UseHsts();
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Serve static files (CSS, JS, images, etc.)
app.UseStaticFiles();

// Enable anti-forgery protection (prevents CSRF attacks)
app.UseAntiforgery();

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------------------------
// MAP RAZOR COMPONENTS
// ----------------------------------------------------------
// Connects the Blazor Server App with its root component (App.razor)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ----------------------------------------------------------
// LOGOUT ENDPOINT
// ----------------------------------------------------------
// This endpoint allows users to log out by clearing authentication cookies
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

#endregion

// Start the application
app.Run();
