using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Envisia.Vite.Extensions.StaticFiles
{
    public class EvViteManifestFileProvider : IFileProvider, IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly PhysicalFileProvider _downstreamFileProvider;
        private readonly string _absoluteRootPath;

        private static readonly char[] PathSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        public EvViteManifestFileProvider(IMemoryCache cache, string absoluteRootPath)
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
            return NullChangeToken.Singleton;
        }

        public void Dispose()
        {
            _downstreamFileProvider?.Dispose();
        }

        private string GetRealPath(string subpath)
        {
            var manifest = GetManifest();
            var strippedPath = subpath.TrimStart(PathSeparators);
            
            // Vite manifest structure is different - it maps entry files to their output info
            // We need to look for the file in the manifest entries
            foreach (var entry in manifest)
            {
                if (entry.Value.TryGetProperty("file", out var file) && file.GetString() == strippedPath)
                {
                    return strippedPath;
                }
                
                // Check if this is the source file being requested
                if (entry.Key == strippedPath && entry.Value.TryGetProperty("file", out var outputFile))
                {
                    return outputFile.GetString();
                }
            }
            
            return subpath;
        }

        private Dictionary<string, JsonElement> GetManifest()
        {
            var manifestJson = Path.Join(_absoluteRootPath, "manifest.json");
            if (_cache.TryGetValue(manifestJson, out var data))
            {
                return data as Dictionary<string, JsonElement>;
            }

            if (!File.Exists(manifestJson))
            {
                return new Dictionary<string, JsonElement>();
            }

            var jsonString = File.ReadAllText(manifestJson);
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);

            _cache.Set(manifestJson, dictionary);
            return dictionary;
        }
    }
}