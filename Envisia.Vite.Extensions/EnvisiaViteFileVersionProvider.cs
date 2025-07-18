using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Envisia.Vite.Extensions
{
    /* Based on: https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Razor/src/Infrastructure/DefaultFileVersionProvider.cs */
    internal class EnvisiaViteFileVersionProvider : IFileVersionProvider
    {
        private const string VersionKey = "v";
        private static readonly char[] QueryStringAndFragmentTokens = { '?', '#' };
        private readonly IFileProvider _fileProvider;
        private readonly IMemoryCache _cache;
        private readonly ISpaStaticFileProvider _spaStaticFileProvider;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EnvisiaViteFileVersionProvider(
            IWebHostEnvironment hostingEnvironment,
            TagHelperMemoryCacheProvider cacheProvider,
            ISpaStaticFileProvider spaStaticFileProvider)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            if (cacheProvider == null)
            {
                throw new ArgumentNullException(nameof(cacheProvider));
            }

            _spaStaticFileProvider = spaStaticFileProvider;

            _fileProvider = hostingEnvironment.WebRootFileProvider;
            _cache = cacheProvider.Cache;
            _hostEnvironment = hostingEnvironment;
        }

        public string AddFileVersionToPath(PathString requestPathBase, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (_hostEnvironment.IsDevelopment())
            {
                return path;
            }

            // specific envisia implementation, that also appends a version to spa files
            var isSpaFile = ExtractSpaFile(requestPathBase, path, out var finalPath);
            var fileProvider = isSpaFile ? _spaStaticFileProvider.FileProvider : _fileProvider;
            var resolvedPath = finalPath;

            var queryStringOrFragmentStartIndex = path.IndexOfAny(QueryStringAndFragmentTokens);
            if (queryStringOrFragmentStartIndex != -1)
            {
                resolvedPath = path.Substring(0, queryStringOrFragmentStartIndex);
            }

            if (Uri.TryCreate(resolvedPath, UriKind.Absolute, out var uri) && !uri.IsFile)
            {
                // Don't append version if the path is absolute.
                return path;
            }

            if (_cache.TryGetValue(path, out string value))
            {
                return value;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions();
            var fileInfo = fileProvider.GetFileInfo(resolvedPath);

            if (!fileInfo.Exists &&
                requestPathBase.HasValue &&
                resolvedPath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
            {
                var requestPathBaseRelativePath = resolvedPath.Substring(requestPathBase.Value.Length);
                fileInfo = fileProvider.GetFileInfo(requestPathBaseRelativePath);
            }

            value = fileInfo.Exists switch
            {
                true when fileInfo.Name != resolvedPath => path.Replace(Path.GetFileName(resolvedPath), fileInfo.Name),
                true => QueryHelpers.AddQueryString(path, VersionKey, GetHashForFile(fileInfo)),
                _ => path
            };

            cacheEntryOptions.SetSize(value.Length * sizeof(char));
            value = _cache.Set(path, value, cacheEntryOptions);
            return value;
        }

        private static bool ExtractSpaFile(PathString requestPathBase, string path, out string finalPath)
        {
            if (requestPathBase.HasValue &&
                path.StartsWith(requestPathBase.Value + "/dist", StringComparison.OrdinalIgnoreCase))
            {
                finalPath = path.Substring((requestPathBase.Value + "/dist").Length);
                return true;
            }

            if (path.StartsWith("/dist", StringComparison.OrdinalIgnoreCase))
            {
                finalPath = path.Substring("/dist".Length);
                return true;
            }

            finalPath = path;
            return false;
        }

        private static string GetHashForFile(IFileInfo fileInfo)
        {
            using var sha256 = CreateSha256();
            using var readStream = fileInfo.CreateReadStream();
            var hash = sha256.ComputeHash(readStream);
            return WebEncoders.Base64UrlEncode(hash);
        }

        private static SHA256 CreateSha256()
        {
            try
            {
                return SHA256.Create();
            }

            // SHA256.Create is documented to throw this exception on FIPS compliant machines.
            // See: https://msdn.microsoft.com/en-us/library/z08hz7ad%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
            catch (System.Reflection.TargetInvocationException)
            {
                // Fallback to a FIPS compliant SHA256 algorithm.
                return new SHA256CryptoServiceProvider();
            }
        }
    }
}