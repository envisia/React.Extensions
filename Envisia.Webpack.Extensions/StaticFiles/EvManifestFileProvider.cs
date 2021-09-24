using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Envisia.Webpack.Extensions.StaticFiles
{
    public class EvManifestFileProvider : IFileProvider, IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly PhysicalFileProvider _downstreamFileProvider;
        private readonly string _absoluteRootPath;

        private static readonly char[] PathSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        public EvManifestFileProvider(IMemoryCache cache, string absoluteRootPath)
        {
            _cache = cache;
            _absoluteRootPath = absoluteRootPath;
            _downstreamFileProvider = new PhysicalFileProvider(absoluteRootPath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var manifest = GetManifest();
            var strippedPath = subpath.TrimStart(PathSeparators);
            return _downstreamFileProvider.GetFileInfo(manifest.TryGetValue(strippedPath, out var finalSubPath)
                ? finalSubPath
                : subpath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _downstreamFileProvider.GetDirectoryContents(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _downstreamFileProvider.Watch(filter);
        }

        public void Dispose()
        {
            _downstreamFileProvider?.Dispose();
        }

        private Dictionary<string, string> GetManifest()
        {
            var manifestJson = Path.Join(_absoluteRootPath, "manifest.json");
            if (_cache.TryGetValue(manifestJson, out var data))
            {
                return data as Dictionary<string, string>;
            }

            if (!File.Exists(manifestJson))
            {
                return new Dictionary<string, string>();
            }

            var reader = new Utf8JsonReader(File.ReadAllBytes(manifestJson));
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader);

            _cache.Set(manifestJson, dictionary);
            return dictionary;
        }
    }
}