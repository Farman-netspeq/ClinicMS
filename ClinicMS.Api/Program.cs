using ClinicMS.Api;
using ClinicMS.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    if (context.HostingEnvironment.IsProduction())
    {
        loggerConfig
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .MinimumLevel.Information();
    }
    else
    {
        loggerConfig.MinimumLevel.Fatal();
    }
});
// ── Services ──────────────────────────────────────────────────
// AddInfrastructure = our extension method from DependencyInjection.cs
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// CORS — allows Web project (different port) to call Api
var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>() ?? throw new InvalidOperationException("CORS AllowedOrigins not configured");
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("WebClient");
// UseCors must come BEFORE UseAuthentication + UseAuthorization

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// ── Seed database on startup ──────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occurred seeding the database");
    }
}

app.Run();