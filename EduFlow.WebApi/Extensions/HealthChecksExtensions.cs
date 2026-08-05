using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using EduFlow.Infrastructure.Database;
using EduFlow.WebApi.HealthChecks;

namespace EduFlow.WebApi.Extensions;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>()
            .AddRedis(configuration.GetConnectionString("redis")!, name: "redis")
            .AddCheck<LocalStorageHealthCheck>("storage");
        return services;
    }

    public static IEndpointRouteBuilder UseHealthChecks(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        return app;
    }
}
