using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Envisia.Vite.Extensions.NodeServices;
using Envisia.Vite.Extensions.NodeServices.Util;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Envisia.Vite.Extensions
{
    internal class EnvisiaViteScriptRunner : BackgroundService
    {
        private const string LogCategoryName = "Microsoft.AspNetCore.SpaServices";
        private readonly IOptions<SpaOptions> _optionsProvider;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly DiagnosticSource _diagnosticSource;
        private readonly ILoggerFactory _loggerFactory;
        private readonly EnvisiaViteBlocker _envisiaViteBlocker;

        private static readonly TimeSpan RegexMatchTimeout =
            TimeSpan.FromSeconds(5); // This is a development-time only feature, so a very long timeout is fine

        private readonly string _scriptName;
        private readonly string _watchMessage;

        public EnvisiaViteScriptRunner(
            string scriptName,
            string watchMessage,
            IOptions<SpaOptions> optionsProvider,
            IHostApplicationLifetime applicationLifetime,
            ILoggerFactory loggerFactory, 
            DiagnosticSource diagnosticSource, 
            EnvisiaViteBlocker envisiaViteBlocker)
        {
            _optionsProvider = optionsProvider;
            _applicationLifetime = applicationLifetime;
            _loggerFactory = loggerFactory;
            _diagnosticSource = diagnosticSource;
            _envisiaViteBlocker = envisiaViteBlocker;
            _scriptName = scriptName;
            _watchMessage = watchMessage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = new SpaOptions
            {
                DefaultPage = _optionsProvider.Value.DefaultPage,
                PackageManagerCommand = _optionsProvider.Value.PackageManagerCommand,
                DefaultPageStaticFileOptions = _optionsProvider.Value.DefaultPageStaticFileOptions,
                SourcePath = _optionsProvider.Value.SourcePath,
                DevServerPort = _optionsProvider.Value.DevServerPort,
            };

            var pkgManagerCommand = options.PackageManagerCommand;
            var sourcePath = options.SourcePath;

            var logger = _loggerFactory.CreateLogger(LogCategoryName);

            var envVars = new Dictionary<string, string>();
            var scriptRunner = new NodeScriptRunner(
                sourcePath,
                _scriptName,
                null,
                envVars,
                pkgManagerCommand,
                _diagnosticSource,
                _applicationLifetime.ApplicationStopping);
            scriptRunner.AttachToLogger(logger);

            using (var stdErrReader = new EventedStreamStringReader(scriptRunner.StdErr))
            {
                try
                {
                    // Vite dev server outputs a specific message when ready
                    // Default watch message for Vite: "ready in"
                    await scriptRunner.StdOut.WaitForMatch(
                        new Regex(_watchMessage, RegexOptions.None, RegexMatchTimeout));
                    _envisiaViteBlocker.CompletionSource.SetResult(true);
                }
                catch (EndOfStreamException ex)
                {
                    throw new InvalidOperationException(
                        $"The {pkgManagerCommand} script '{_scriptName}' exited without indicating that the " +
                        $"Vite dev server was ready. The error output was: " +
                        $"{stdErrReader.ReadAsString()}", ex);
                }
            }

            await Task.CompletedTask;
        }
    }
}