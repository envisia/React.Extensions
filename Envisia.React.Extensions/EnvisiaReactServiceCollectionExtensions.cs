using System;
using JavaScriptEngineSwitcher.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using React;
using React.AspNet;

namespace Envisia.React.Extensions
{
    /// <summary>
    /// Handles registering ReactJS.NET services in the ASP.NET <see cref="IServiceCollection"/>.
    /// </summary>
    public static class EnvisiaReactServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all services required for ReactJS.NE
        /// </summary>
        /// <param name="services">ASP.NET services</param>
        /// <param name="configure">ReactJS.NET configuration</param>
        /// <returns>The application builder (for chaining)</returns>
        public static IServiceCollection AddReactCore(
            this IServiceCollection services,
            Action<IReactSiteConfiguration> configure,
            bool devMode = false)
        {
            configure(ReactSiteConfiguration.Configuration);

            services.AddSingleton(provider => ReactSiteConfiguration.Configuration);
            services.AddScoped<IFileCacheHash, FileCacheHash>();
            services.AddSingleton(provider => JsEngineSwitcher.Current);
            services.AddSingleton<IJavaScriptEngineFactory, JavaScriptEngineFactory>();
            services.AddSingleton<IReactIdGenerator, ReactIdGenerator>();
            services.AddScoped<IReactEnvironment, ReactEnvironment>();
            services.AddSingleton<IFileSystem, AspNetFileSystem>();

            if (devMode)
            {
                services.AddSingleton<ICache, NullCache>();
            }
            else
            {
                services.AddSingleton<ICache, MemoryFileCacheCore>();
            }

            // Camelcase JSON properties by default - Can be overridden per-site in "configure".
            ReactSiteConfiguration.Configuration.JsonSerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();

            return services;
        }
    }
}