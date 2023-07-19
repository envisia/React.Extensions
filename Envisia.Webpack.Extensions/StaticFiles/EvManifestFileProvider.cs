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
            return _downstreamFileProvider.GetFileInfo(GetRealPath(subpath));
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _downstreamFileProvider.GetDirectoryContents(GetRealPath(subpath));
        }

        public IChangeToken Watch(string filter)
        {
            // we do not want to poll for files in production...
            // never ever!!
            return NullChangeToken.Singleton;;
        }

        public void Dispose()
        {
            _downstreamFileProvider?.Dispose();
        }

        private string GetRealPath(string subpath)
        {
            var manifest = GetManifest();
            var strippedPath = subpath.TrimStart(PathSeparators);
            return manifest.TryGetValue(strippedPath, out var finalSubPath) ? finalSubPath : subpath;
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