using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

#region Swagger Configuration
/// <summary>
/// Configures Swagger (OpenAPI) for API documentation and testing.
/// </summary>
/// <remarks>
/// This configuration:
/// 1. Adds a Bearer authentication scheme for JWT token input in Swagger UI.
/// 2. Applies security globally to all endpoints.
/// 3. Allows developers to test authenticated APIs directly from the Swagger UI.
/// </remarks>
builder.Services.AddSwaggerGen(options =>
{
    // Define the Bearer token security scheme
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header, // JWT will be passed in the HTTP header
        Description = "Please enter a valid JWT token in the format: Bearer {token}",
        Name = "Authorization", // The header key used for JWT authentication
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    // Apply this security definition globally
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

#region Logging Configuration
/// <summary>
/// Sets the minimum logging level for the application.
/// </summary>
/// <remarks>
/// Debug level ensures detailed logs for development and troubleshooting.
/// </remarks>
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endregion

#region Authorization Policies
/// <summary>
/// Configures scope-based authorization policies.
/// </summary>
/// <remarks>
/// Policies defined:
/// - EmailScope: Requires "email" scope.
/// - AdminScope: Requires "admin.sports.api" scope.
/// - ManagerScope: Requires "manager.sports.api" scope.
/// - UserScope: Requires "user.sports.api" scope.
/// </remarks>
builder.Services.AddAuthorization(options =>
{
    // Email scope policy
    options.AddPolicy("EmailScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.Scope, Scopes.Email);
    });

    // Admin-level scope policy
    options.AddPolicy("AdminScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "scope" &&
                (c.Value == "admin.sports.api" || c.Value.Split(' ').Contains("admin.sports.api"))));
    });

    // Manager-level scope policy
    options.AddPolicy("ManagerScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "scope" &&
                (c.Value == "manager.sports.api" || c.Value.Split(' ').Contains("manager.sports.api"))));
    });

    // User-level scope policy
    options.AddPolicy("UserScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "scope" &&
                (c.Value == "user.sports.api" || c.Value.Split(' ').Contains("user.sports.api"))));
    });
});
#endregion

#region Authentication Configuration
/// <summary>
/// Registers authentication using OpenIddict's validation middleware.
/// </summary>
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
#endregion

#region OpenIddict Token Validation
/// <summary>
/// Configures OpenIddict to validate tokens from the identity server.
/// </summary>
/// <remarks>
/// - Validates token issuer.
/// - Ensures token audience matches the API.
/// - Integrates with ASP.NET Core authentication pipeline.
/// </remarks>
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("https://localhost:7214/"); // Identity server issuer URL
        options.AddAudiences("SportsApi");           // Valid audience for this API
        options.UseSystemNetHttp();                  // Use System.Net.Http for introspection
        options.UseAspNetCore();                     // Integrates authentication into ASP.NET Core
    });
#endregion

#region CORS Configuration
/// <summary>
/// Configures Cross-Origin Resource Sharing (CORS).
/// </summary>
/// <remarks>
/// - Reads allowed origins from appsettings.json.
/// - Enables Blazor or other client applications to call this API.
/// </remarks>
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(allowedOrigins) // Trusted client URLs
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
#endregion

#region Controllers and API Services
/// <summary>
/// Registers controllers, endpoints, and Swagger support.
/// </summary>
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

var app = builder.Build();

#region Middleware: Debug Logging
/// <summary>
/// Middleware to log JWT authentication and user claims.
/// </summary>
/// <remarks>
/// - Logs Authorization header if present.
/// - Displays authentication status and claims for debugging.
/// </remarks>
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"Authorization header: {authHeader}");
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            Console.WriteLine($"User authenticated: {context.User.Identity.Name}");
            foreach (var claim in context.User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }
        }
        else
        {
            Console.WriteLine("User not authenticated");
        }
    }
    await next();
});
#endregion

#region Middleware Pipeline
/// <summary>
/// Configures the HTTP request pipeline.
/// </summary>
/// <remarks>
/// - Enables Swagger in development.
/// - Enforces HTTPS.
/// - Applies CORS, authentication, and authorization.
/// - Maps API endpoints.
/// </remarks>
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();

/// <summary>
/// Root endpoint for API health check.
/// </summary>
app.MapGet("/", () => "Sports Api is running");

// Map controllers for API endpoints
app.MapControllers();
#endregion

app.Run();
