using FormMaker.Api.Data;
using FormMaker.Api.Middleware;
using FormMaker.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register CORS middleware
builder.Services.AddSingleton<IFunctionsWorkerMiddleware, CorsMiddleware>();

// Add DbContext with SQLite for local development or PostgreSQL for production
var connectionString = builder.Configuration.GetValue<string>("Values:ConnectionStrings:FormMakerDb")
    ?? builder.Configuration.GetValue<string>("ConnectionStrings:FormMakerDb")
    ?? "Data Source=formmaker.db";

builder.Services.AddDbContext<FormMakerDbContext>(options =>
{
    if (connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://") || connectionString.Contains("Host="))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Register JWT Token Service
// For development, use a default secret if not configured
var jwtSecret = builder.Configuration.GetValue<string>("Values:JwtSecret")
    ?? builder.Configuration.GetValue<string>("JwtSecret")
    ?? "your-secret-key-min-32-characters-long-for-development";
var jwtIssuer = builder.Configuration.GetValue<string>("Values:JwtIssuer")
    ?? builder.Configuration.GetValue<string>("JwtIssuer")
    ?? "FormMaker";
var jwtAudience = builder.Configuration.GetValue<string>("Values:JwtAudience")
    ?? builder.Configuration.GetValue<string>("JwtAudience")
    ?? "FormMakerClient";

builder.Services.AddSingleton(sp => new TokenService(jwtSecret, jwtIssuer, jwtAudience));

// Register Auth Service
builder.Services.AddScoped<AuthService>();

// Register Template Service
builder.Services.AddScoped<TemplateService>();

// Register Form Service
builder.Services.AddScoped<FormService>();

// Register Submission Service
builder.Services.AddScoped<SubmissionService>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

var host = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<FormMakerDbContext>();
        logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

host.Run();
