using System;
using Envisia.Webpack.Extensions;
using Envisia.Webpack.Extensions.StaticFiles;
using JavaScriptEngineSwitcher.Core;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
            Action<IReactSiteConfiguration> configure)
        {
            configure(ReactSiteConfiguration.Configuration);

            services.AddSingleton(provider => ReactSiteConfiguration.Configuration);
            services.AddSingleton(provider => JsEngineSwitcher.Current);
            services.AddSingleton<IJavaScriptEngineFactory, JavaScriptEngineFactory>();
            services.AddSingleton<IReactIdGenerator, ReactIdGenerator>();
            services.AddScoped<IReactEnvironment, ReactEnvironment>();
            services.AddSingleton<IFileSystem, AspNetFileSystem>();
            services.AddScoped<IFileCacheHash, FileCacheHash>();
            services.AddSingleton<ICache, MemoryFileCacheCore>();

            services.AddHtmlStaticFileVersion();

            // Camelcase JSON properties by default - Can be overridden per-site in "configure".
            ReactSiteConfiguration.Configuration.JsonSerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();

            return services;
        }
    }
}