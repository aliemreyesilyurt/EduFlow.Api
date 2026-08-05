using EduFlow.Application.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EduFlow.WebApi.HealthChecks;

public sealed class LocalStorageHealthCheck(IHostEnvironment environment, IOptions<StorageOptions> options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var rootPath = Path.IsPathRooted(options.Value.RootPath)
            ? options.Value.RootPath
            : Path.Combine(environment.ContentRootPath, options.Value.RootPath);

        try
        {
            Directory.CreateDirectory(rootPath);
            var probePath = Path.Combine(rootPath, $".health-{Guid.NewGuid():N}");
            File.WriteAllText(probePath, string.Empty);
            File.Delete(probePath);
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Storage root is not writable", ex));
        }
    }
}
