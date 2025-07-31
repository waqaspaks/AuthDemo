using AuthDemo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Identity",
        Description = "API for Auth Demo with OpenIddict and JWT support"
    });

    // Enable XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict();
});

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure OpenIddict
builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy("ApiScope", policy =>
    //{
    //    policy.RequireAuthenticatedUser();
    //    policy.RequireAssertion(context =>
    //        context.User.HasClaim(c => c.Type == "scope" &&
    //            (c.Value == "api" || c.Value.Split(' ').Contains("api"))));
    //});
    options.AddPolicy("EmailScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.Scope, Scopes.Email);
    });
    options.AddPolicy("AdminScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "admin.demo.api") ||
            context.User.HasClaim("scope", "admin.mock.api")
        );
    });

    options.AddPolicy("ManagerScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("scope", "manager.demo.api") ||
            context.User.HasClaim("scope", "manager.mock.api")
        );
    });

    options.AddPolicy("UserScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireScope("user.demo.api");
    });
});
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token")
               .SetAuthorizationEndpointUris("/connect/authorize");

        // Userinfo endpoint is not available in v7 by default. Remove or implement manually if needed.
        options.SetIssuer("https://localhost:7214/");
        options.AllowPasswordFlow()
               .AllowClientCredentialsFlow()
               .AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

        options.RegisterScopes("admin.demo.api", "manager.demo.api", "admin.mock.api", "manager.mock.api", "user.demo.api");
        options.RegisterAudiences("DemoApi", "MockApi");

        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate()
               .DisableAccessTokenEncryption();


        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableAuthorizationEndpointPassthrough();

        options.UseAspNetCore();
    });
    //.AddValidation(options =>
    //{
    //    var issuer = builder.Configuration["OpenIddict:Issuer"];
    //    options.SetIssuer("https://localhost:7214/"); // Must match token issuer
    //    options.UseLocalServer();
    //    options.UseAspNetCore();
    //});

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
// Debug logging middleware for incoming JWTs
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"Authorization header: {authHeader}");
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            Console.WriteLine($"User authenticated: {context.User.Identity.Name}");
            foreach (var claim1 in context.User.Claims)
            {
                Console.WriteLine($"Claim: {claim1.Type} = {claim1.Value}");
            }
        }
        else
        {
            Console.WriteLine("User not authenticated");
        }
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/", () => "Auth API is running");

app.MapControllers();

// Initialize the database with seed data
using (var scope = app.Services.CreateScope())
{
    await AuthDemo.Services.SeedData.InitializeAsync(scope.ServiceProvider);
    await AuthDemo.Services.SeedData.SeedRolesAndUsers(scope.ServiceProvider);
}

app.Run();
