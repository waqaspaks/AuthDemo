using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

#region Swagger Configuration
/// <summary>
/// Configures Swagger (OpenAPI) with JWT Bearer authentication support.
/// </summary>
builder.Services.AddSwaggerGen(options =>
{
    // Add Bearer token authentication definition for Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter a valid JWT token using the format: Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    // Apply the Bearer token requirement to all endpoints
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

#region Logging
/// <summary>
/// Configures logging to include debug-level logs.
/// </summary>
builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endregion

#region Authorization Policies
/// <summary>
/// Adds authorization policies for various scopes such as Email, Admin, Manager, and User.
/// </summary>
builder.Services.AddAuthorization(options =>
{
    // Email Scope Policy
    options.AddPolicy("EmailScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.Scope, Scopes.Email);
    });

    // Admin Scope Policy
    options.AddPolicy("AdminScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "scope" &&
                (c.Value == "admin.transport.api" || c.Value.Split(' ').Contains("admin.transport.api"))));
    });

    // Manager Scope Policy
    options.AddPolicy("ManagerScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "scope" &&
                (c.Value == "manager.transport.api" || c.Value.Split(' ').Contains("manager.transport.api"))));
    });

    // User Scope Policy
    options.AddPolicy("UserScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "scope" &&
                (c.Value == "user.transport.api" || c.Value.Split(' ').Contains("user.transport.api"))));
    });
});
#endregion

#region Authentication & OpenIddict Validation
/// <summary>
/// Configures OpenIddict token validation with JWT support for the Transport API.
/// </summary>
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

var issuer = builder.Configuration["OpenIddict:Issuer"]; // Token issuer URL

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(issuer);                   // Set the expected issuer
        options.AddAudiences("TransportApi");        // Restrict to the TransportApi audience
        options.UseSystemNetHttp();                  // Use System.Net.Http for token introspection
        options.UseAspNetCore();                     // Integrate with ASP.NET Core authentication middleware
    });
#endregion

#region CORS Configuration
/// <summary>
/// Configures CORS to allow the Blazor client to access the API.
/// </summary>
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
#endregion

#region MVC and Swagger Registration
/// <summary>
/// Adds MVC controllers and Swagger/OpenAPI support.
/// </summary>
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

var app = builder.Build();

#region Request Logging Middleware
/// <summary>
/// Middleware that logs the Authorization header and user claims for debugging purposes.
/// </summary>
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

    if (!string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"Authorization header: {authHeader}");

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            Console.WriteLine($"User authenticated: {context.User.Identity.Name}");

            // Log all user claims for debugging
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

#region Development Tools
/// <summary>
/// Enables Swagger UI in development mode.
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
#endregion

#region Middleware Pipeline
/// <summary>
/// Configures middleware for HTTPS redirection, CORS, authentication, and authorization.
/// </summary>
app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
#endregion

#region Health Check Endpoint
/// <summary>
/// Health check endpoint for confirming that the API is running.
/// </summary>
app.MapGet("/", () => "SecureDemoApi is running");
#endregion

#region Controller Endpoints
/// <summary>
/// Maps controller endpoints.
/// </summary>
app.MapControllers();
#endregion

// Run the application
app.Run();
