using AuthDemo.Transport.Components;
using AuthDemo.Transport.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

#region Services Configuration

// ===========================================
//  RAZOR COMPONENTS & BLAZOR SERVER SETUP
// ===========================================
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===========================================
//  AUTHENTICATION STATE PROVIDER
//  Provides the current user's authentication state to Blazor components
// ===========================================
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddSingleton<TokenHolder>(); // Holds token and authentication info

// ===========================================
//  COOKIE AUTHENTICATION
//  Configures authentication scheme for login sessions
// ===========================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        var config = builder.Configuration;

        // Login & Access Denied redirect paths
        options.LoginPath = config["Auth:LoginPath"] ?? "/login";
        options.AccessDeniedPath = config["Auth:AccessDeniedPath"] ?? "/accessdenied";

        // Cookie expiration settings
        options.ExpireTimeSpan = TimeSpan.FromMinutes(
            int.TryParse(config["Auth:ExpireMinutes"], out var min) ? min : 30
        );
        options.SlidingExpiration = true; // Refresh expiration on activity
    });

// ===========================================
//  ROLE-BASED AUTHORIZATION POLICIES
//  These policies enforce role checks
// ===========================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    // Additional role policies can be added here if needed
});

// ===========================================
//  SCOPE-BASED AUTHORIZATION POLICIES
//  These policies enforce access based on OAuth2 scopes
// ===========================================
builder.Services.AddAuthorization(options =>
{
    // Admin policy allows either transport or sports admin scope
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "admin.transport.api") ||
            context.User.HasClaim("scope", "admin.sports.api")
        );
    });

    // Manager policy allows either transport or sports manager scope
    options.AddPolicy("ManagerPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "manager.transport.api") ||
            context.User.HasClaim("scope", "manager.sports.api")
        );
    });

    // User policy allows only sports user scope
    options.AddPolicy("UserPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "user.sports.api")
        );
    });
});

// ===========================================
//  SERVICE DEPENDENCY INJECTION
// ===========================================
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<FlightService>();
builder.Services.AddScoped<BusService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<IApiClientFactory, ApiClientFactory>();

// ===========================================
//  HTTP CLIENTS CONFIGURATION
//  Separate HttpClients for different APIs
// ===========================================
var transportApiBaseUrl = builder.Configuration["Api:transportApiBaseUrl"] ?? "https://localhost:7389";
var sportsApiBaseUrl = builder.Configuration["Api:sportsApiBaseUrl"] ?? "https://localhost:7236";
var authApiBaseUrl = builder.Configuration["Api:AuthApiBaseUrl"] ?? "https://localhost:7214";

// Auth API Client
builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri(authApiBaseUrl);
});

// Transport API Client
builder.Services.AddHttpClient("TransportApi", client =>
{
    client.BaseAddress = new Uri(transportApiBaseUrl);
});

// Sports API Client
builder.Services.AddHttpClient("SportsApi", client =>
{
    client.BaseAddress = new Uri(sportsApiBaseUrl);
});

#endregion

var app = builder.Build();

#region Middleware Pipeline

// ===========================================
//  ERROR HANDLING & SECURITY
// ===========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts(); // Enable HSTS for production
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery(); // Protect against CSRF attacks

// ===========================================
//  AUTHENTICATION & AUTHORIZATION
// ===========================================
app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Endpoint Mapping

// ===========================================
//  BLAZOR COMPONENTS ENDPOINT
// ===========================================
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ===========================================
//  LOGOUT ENDPOINT
//  Clears authentication cookie and redirects to home
// ===========================================
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

#endregion

app.Run();
