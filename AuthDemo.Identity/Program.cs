using AuthDemo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Adds controller support for handling API endpoints.
/// </summary>
builder.Services.AddControllers();

/// <summary>
/// Adds Swagger/OpenAPI for API documentation and testing.
/// </summary>
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Identity API",
        Description = "API for Auth Demo with OpenIddict and JWT authentication support"
    });

    // Include XML documentation if available (for better Swagger descriptions).
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

/// <summary>
/// Configures Entity Framework Core to use SQL Server and registers OpenIddict support.
/// </summary>
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict(); // Enables OpenIddict integration with EF Core
});

/// <summary>
/// Configures ASP.NET Identity for user and role management.
/// </summary>
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password policy settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

/// <summary>
/// Defines authorization policies based on user scopes.
/// These policies ensure access control based on assigned scopes in the JWT.
/// </summary>
builder.Services.AddAuthorization(options =>
{
    // Policy to check for email scope
    options.AddPolicy("EmailScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.Scope, Scopes.Email);
    });

    // Policy to check for admin access to transport or sports APIs
    options.AddPolicy("AdminScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "admin.transport.api") ||
            context.User.HasClaim("scope", "admin.sports.api")
        );
    });

    // Policy to check for manager access to transport or sports APIs
    options.AddPolicy("ManagerScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "manager.transport.api") ||
            context.User.HasClaim("scope", "manager.sports.api")
        );
    });

    // Policy to check for user access to transport API
    options.AddPolicy("UserScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireScope("user.transport.api");
    });
});

/// <summary>
/// Configures authentication to use OpenIddict validation scheme.
/// </summary>
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

/// <summary>
/// Configures OpenIddict for issuing and validating tokens.
/// </summary>
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        // Token & authorization endpoint URIs
        options.SetTokenEndpointUris("/connect/token")
               .SetAuthorizationEndpointUris("/connect/authorize");

        // Set the issuer (must match the API URL)
        options.SetIssuer("https://localhost:7214/");

        // Allowed authentication flows
        options.AllowPasswordFlow()
               .AllowClientCredentialsFlow()
               .AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

        // Register available scopes for APIs
        options.RegisterScopes("admin.transport.api", "manager.transport.api", "admin.sports.api", "manager.sports.api", "user.transport.api");

        // Register API audiences for JWT validation
        options.RegisterAudiences("TransportApi", "SportsApi");

        // Add development certificates (for signing and encryption)
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate()
               .DisableAccessTokenEncryption();

        // Enable ASP.NET Core endpoint integration
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableAuthorizationEndpointPassthrough();
    });

/// <summary>
/// Adds CORS policy to allow requests from the Blazor client.
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

var app = builder.Build();

/// <summary>
/// Middleware to log JWT authentication details for debugging.
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

/// <summary>
/// Enables Swagger UI in Development mode.
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/// <summary>
/// Middleware pipeline setup.
/// </summary>
app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();

/// <summary>
/// Health check endpoint to verify the API is running.
/// </summary>
app.MapGet("/", () => "Auth API is running");

/// <summary>
/// Maps all controller endpoints.
/// </summary>
app.MapControllers();

/// <summary>
/// Initializes the database with seed data (roles, users, etc.).
/// </summary>
using (var scope = app.Services.CreateScope())
{
    await AuthDemo.Services.SeedData.InitializeAsync(scope.ServiceProvider);
    await AuthDemo.Services.SeedData.SeedRolesAndUsers(scope.ServiceProvider);
}

app.Run();
