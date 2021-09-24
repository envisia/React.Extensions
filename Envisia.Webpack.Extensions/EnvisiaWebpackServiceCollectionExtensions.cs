using Envisia.Webpack.Extensions.StaticFiles;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Envisia.Webpack.Extensions
{
    public static class EnvisiaWebpackServiceCollectionExtensions
    {
        public static void AddHtmlStaticFileVersion(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IFileVersionProvider, EnvisiaReactFileVersionProvider>();
            
            services.AddEvSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }
    }
}