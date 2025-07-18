using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Envisia.Vite.Extensions.StaticFiles
{
    public class EvViteStaticFileProvider : ISpaStaticFileProvider
    {
        private readonly IFileProvider _fileProvider;

        public EvViteStaticFileProvider(
            IServiceProvider serviceProvider,
            SpaStaticFilesOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.RootPath))
            {
                throw new ArgumentException($"The {nameof(options.RootPath)} property " +
                                            $"of {nameof(options)} cannot be null or empty.");
            }

            var cacheProvider = serviceProvider.GetRequiredService<IMemoryCache>();

            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            var absoluteRootPath = Path.Combine(
                env.ContentRootPath,
                options.RootPath);

            // PhysicalFileProvider will throw if you pass a non-existent path,
            // but we don't want that scenario to be an error because for SPA
            // scenarios, it's better if non-existing directory just means we
            // don't serve any static files.
            if (Directory.Exists(absoluteRootPath))
            {
                if (env.IsDevelopment())
                {
                    _fileProvider = new PhysicalFileProvider(absoluteRootPath);
                }
                else
                {
                    _fileProvider = new EvViteManifestFileProvider(cacheProvider, absoluteRootPath);
                }
            }
            else
            {
                _fileProvider = new NullFileProvider();
            }
        }

        public IFileProvider FileProvider => _fileProvider;
    }
}