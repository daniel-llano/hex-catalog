using System.Text;
using System.Threading.RateLimiting;
using HexCatalog.Api.Extensions;
using HexCatalog.Api.Infrastructure;
using HexCatalog.Application;
using HexCatalog.Infrastructure;
using HexCatalog.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", failureStatus: HealthStatus.Unhealthy, tags: ["ready"]);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(config);

var jwtSecret = RequireJwtSecret(config, builder.Environment);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromMinutes(1), PermitLimit = config.GetValue("RateLimiting:AuthPermitsPerMinute", 20), QueueLimit = 0 }));
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HexCatalog API",
        Version = "v1",
        Description = "Product catalog with JWT auth, refresh tokens, and stock reservations."
    });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "HexCatalog.Api.xml"));
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Paste the access token from /api/auth/login.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = []
    });
});

var corsOrigins = config.GetSection("Cors:Origins").Get<string[]>() ?? [];
if (corsOrigins.Contains("*"))
    throw new InvalidOperationException("Cors:Origins can't be *. List the admin UI origins.");
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

await app.ApplyMigrationsIfConfiguredAsync();
if (config.GetValue<bool>("UseInMemoryDatabase"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
}
await DataSeeder.SeedAsync(app.Services, config);

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsProduction())
    app.UseHsts();

if (app.Environment.IsDevelopment() || config.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HexCatalog v1"));
}

if (!ServesHttpOnly(app))
    app.UseHttpsRedirection();

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapHealthChecks("/health");

app.Run();

static string RequireJwtSecret(IConfiguration config, IHostEnvironment env)
{
    var secret = config["Jwt:Secret"];
    if (string.IsNullOrWhiteSpace(secret))
        throw new InvalidOperationException("Jwt:Secret is required. Set Jwt__Secret in the environment.");
    if (secret.Length < 32)
        throw new InvalidOperationException("Jwt:Secret must be at least 32 characters.");
    if (env.IsProduction() && secret.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Jwt:Secret is still the placeholder. Set a real secret.");
    return secret;
}

// Containers and local http-only runs sit behind a proxy that terminates TLS, so don't redirect.
static bool ServesHttpOnly(WebApplication app)
{
    if (app.Environment.IsEnvironment("Testing"))
        return true;
    var urls = app.Configuration["ASPNETCORE_URLS"] ?? app.Configuration["urls"] ?? "";
    return urls.Contains("http://", StringComparison.OrdinalIgnoreCase)
        && !urls.Contains("https://", StringComparison.OrdinalIgnoreCase);
}

public partial class Program;
