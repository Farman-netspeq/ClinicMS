using ClinicMS.Api;
using ClinicMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────
// AddInfrastructure = our extension method from DependencyInjection.cs
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// CORS — allows Web project (different port) to call Api
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        policy.WithOrigins(
        "https://localhost:7145",
        "http://localhost:5186")
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