using System;
using Envisia.Webpack.Extensions;
using Envisia.Webpack.Extensions.StaticFiles;
using JavaScriptEngineSwitcher.Core;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using React;
using React.AspNet;
using AssemblyRegistration = React.AssemblyRegistration;

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
        /// <param name="react18">ReactJS 18 support</param>
        /// <returns>The application builder (for chaining)</returns>
        public static IServiceCollection AddReactCore(
            this IServiceCollection services,
            Action<IReactSiteConfiguration> configure,
            bool react18)
        {
            configure(ReactSiteConfiguration.Configuration);

            services.AddSingleton(provider => ReactSiteConfiguration.Configuration);
            services.AddSingleton(provider => JsEngineSwitcher.Current);
            services.AddSingleton<IJavaScriptEngineFactory, JavaScriptEngineFactory>();
            services.AddSingleton<IReactIdGenerator, ReactIdGenerator>();
            if (react18)
            {
                services.AddScoped<IReactEnvironment, React18Environment>();
            }
            else
            {
                services.AddScoped<IReactEnvironment, ReactEnvironment>();
            }

            services.AddSingleton<IFileSystem, AspNetFileSystem>();
            services.AddScoped<IFileCacheHash, FileCacheHash>();
            services.AddSingleton<ICache, MemoryFileCacheCore>();

            services.AddHtmlStaticFileVersion();

            if (react18)
            {
                AssemblyRegistration.Container
                    .Unregister<IReactEnvironment>();
                AssemblyRegistration.Container
                    .Register<IReactEnvironment, React18Environment>()
                    .AsPerRequestSingleton();
            }

            // Camelcase JSON properties by default - Can be overridden per-site in "configure".
            ReactSiteConfiguration.Configuration.JsonSerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();

            return services;
        }
    }
}