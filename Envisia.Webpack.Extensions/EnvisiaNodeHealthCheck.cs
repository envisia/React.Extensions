using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Envisia.Webpack.Extensions
{
    internal class EnvisiaNodeHealthCheck(EnvisiaNodeBlocker envisiaNodeBlocker) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var result = envisiaNodeBlocker.CompletionSource.Task.IsCompletedSuccessfully
                ? HealthCheckResult.Healthy("Node script runner is ready.")
                : HealthCheckResult.Unhealthy("Node script runner is not ready.");

            return Task.FromResult(result);
        }
    }
}
