using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

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

builder.Logging.SetMinimumLevel(LogLevel.Debug);
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
        context.User.HasClaim(c => c.Type == "scope" &&
            (c.Value == "admin.mock.api" || c.Value.Split(' ').Contains("admin.mock.api"))));
    });

    options.AddPolicy("ManagerScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
             context.User.HasClaim(c => c.Type == "scope" &&
            (c.Value == "manager.mock.api" || c.Value.Split(' ').Contains("manager.mock.api"))));
    });

    options.AddPolicy("UserScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
             context.User.HasClaim(c => c.Type == "scope" &&
            (c.Value == "user.mock.api" || c.Value.Split(' ').Contains("user.mock.api"))));
    });
});
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
var issuer = builder.Configuration["OpenIddict:Issuer"];

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("https://localhost:7214/");
        options.AddAudiences("MockApi");
        //options.UseLocalServer();
        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
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
// Configure the HTTP request pipeline.
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
app.MapGet("/", () => "MockApiService is running");

app.MapControllers();

app.Run();
