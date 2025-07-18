using Envisia.Vite.Extensions.StaticFiles;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Envisia.Vite.Extensions
{
    public static class EnvisiaViteServiceCollectionExtensions
    {
        public static void AddDevelopmentModeViteDev(
            this IServiceCollection services,
            string scriptName = "dev",
            bool isDevelopment = false,
            string watchMessage = "ready in")
        {
            if (!isDevelopment) return;

            services.AddSingleton(provider =>
                ActivatorUtilities.CreateInstance<EnvisiaViteScriptRunner>(provider, scriptName, watchMessage));
            services.AddSingleton<EnvisiaViteBlocker>();
            services.AddHostedService(provider => provider.GetRequiredService<EnvisiaViteScriptRunner>());
        }

        public static void AddViteStaticFileVersion(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IFileVersionProvider, EnvisiaViteFileVersionProvider>();

            services.AddEvViteSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
        }
    }
}