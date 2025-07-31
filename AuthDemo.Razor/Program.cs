using AuthDemo.Razor.Components;
using AuthDemo.Razor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddSingleton<TokenHolder>();

// Add Authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        var config = builder.Configuration;
        options.LoginPath = config["Auth:LoginPath"] ?? "/login";
        options.AccessDeniedPath = config["Auth:AccessDeniedPath"] ?? "/accessdenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(int.TryParse(config["Auth:ExpireMinutes"], out var min) ? min : 30);
        options.SlidingExpiration = true;
    });

// Add Authorization services with role policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    // Add more policies as needed
});

// Make AuthenticationState available to Blazor components
builder.Services.AddCascadingAuthenticationState();

// Optional: Add HttpContextAccessor if you need to access HttpContext directly in services
// builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<FlightService>();
builder.Services.AddScoped<GameMatchService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<IApiClientFactory, ApiClientFactory>();

// Configure HttpClient using BaseUrl from appsettings
var demoApiBaseUrl = builder.Configuration["Api:demoApiBaseUrl"] ?? "https://localhost:7389";
var mockApiBaseUrl = builder.Configuration["Api:mockApiBaseUrl"] ?? "https://localhost:7236";
var AuthApiBaseUrl = builder.Configuration["Api:AuthApiBaseUrl"] ?? "https://localhost:7214";

builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri(AuthApiBaseUrl);
});

builder.Services.AddHttpClient("DemoApi", client =>
{
    client.BaseAddress = new Uri(demoApiBaseUrl);
});

builder.Services.AddHttpClient("MockApi", client =>
{
    client.BaseAddress = new Uri(mockApiBaseUrl);
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "admin.demo.api") ||
            context.User.HasClaim("scope", "admin.mock.api")
        );
    });

    options.AddPolicy("ManagerPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "manager.demo.api") ||
            context.User.HasClaim("scope", "manager.mock.api")
        );
    });

    options.AddPolicy("UserPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "user.mock.api")
        );
    });
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});


app.Run();
