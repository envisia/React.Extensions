using Envisia.Webpack.Extensions.StaticFiles;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;

namespace Envisia.Webpack.Extensions
{
    public static class EnvisiaWebpackServiceCollectionExtensions
    {
        public static void AddNodeRunner(
            this IServiceCollection services,
            string scriptName = "build:dev",
            bool isDevelopment = false,
            string watchMessage = "Starting Watch Mode...")
        {
            if (!isDevelopment) return;

            services.AddSingleton(provider =>
                ActivatorUtilities.CreateInstance<EnvisiaNodeScriptRunner>(provider, scriptName, watchMessage));
            services.AddSingleton<EnvisiaNodeBlocker>();
            services.AddHostedService(provider => provider.GetRequiredService<EnvisiaNodeScriptRunner>());
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