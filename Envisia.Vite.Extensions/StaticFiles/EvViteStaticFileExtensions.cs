using System;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Envisia.Vite.Extensions.StaticFiles
{
    public static class EvViteStaticFileExtensions
    {
        public static void AddEvViteSpaStaticFiles(
            this IServiceCollection services,
            Action<SpaStaticFilesOptions> configuration = null)
        {
            services.AddSingleton<ISpaStaticFileProvider>(serviceProvider =>
            {
                // Use the options configured in DI (or blank if none was configured)
                var optionsProvider = serviceProvider.GetService<IOptions<SpaStaticFilesOptions>>();
                var options = optionsProvider.Value;

                // Allow the developer to perform further configuration
                configuration?.Invoke(options);

                if (string.IsNullOrEmpty(options.RootPath))
                {
                    throw new InvalidOperationException($"No {nameof(SpaStaticFilesOptions.RootPath)} " +
                                                        $"was set on the {nameof(SpaStaticFilesOptions)}.");
                }

                return new EvViteStaticFileProvider(serviceProvider, options);
            });
        }
    }
}