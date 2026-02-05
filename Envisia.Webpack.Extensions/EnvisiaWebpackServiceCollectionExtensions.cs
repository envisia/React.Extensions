using System.Collections.Generic;
using Envisia.Webpack.Extensions.StaticFiles;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Envisia.Webpack.Extensions
{
    public static class EnvisiaWebpackServiceCollectionExtensions
    {
        public static void AddNodeRunner(
            this IServiceCollection services,
            string scriptName = "build:dev",
            bool condition = false,
            string watchMessage = "Starting Watch Mode...")
        {
            if (!condition) return;

            services.AddSingleton(provider =>
                ActivatorUtilities.CreateInstance<EnvisiaNodeScriptRunner>(provider, scriptName, watchMessage));
            services.AddSingleton<EnvisiaNodeBlocker>();
            services.AddHostedService(provider => provider.GetRequiredService<EnvisiaNodeScriptRunner>());
            services
                .AddHealthChecks()
                .AddCheck<EnvisiaNodeHealthCheck>(
                    "node-script-runner", 
                    tags: ["ready"],
                    failureStatus: HealthStatus.Unhealthy);

            services.Configure<SpaOptions>(options =>
            {
                options.SourcePath = "ClientApp";
                options.PackageManagerCommand = "npm";
            });
            services.Configure<EvSpaOptions>(options =>
            {
                options.PackageManagerScript = "run";
            });
        }

        public static void AddHtmlStaticFileVersion(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IFileVersionProvider, EnvisiaReactFileVersionProvider>();

            services.AddEvSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
        }
    }
}