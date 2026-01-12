using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Envisia.Webpack.Extensions.NodeServices;
using Envisia.Webpack.Extensions.NodeServices.Util;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Envisia.Webpack.Extensions
{
    internal class EnvisiaNodeScriptRunner : BackgroundService
    {
        private const string LogCategoryName = "Microsoft.AspNetCore.SpaServices";
        private readonly IOptions<SpaOptions> _optionsProvider;
        private readonly IOptions<EvSpaOptions> _envisiaOptionsProvider;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly DiagnosticSource _diagnosticSource;
        private readonly ILoggerFactory _loggerFactory;
        private readonly EnvisiaNodeBlocker _envisiaNodeBlocker;

        private static readonly TimeSpan RegexMatchTimeout =
            TimeSpan.FromSeconds(5); // This is a development-time only feature, so a very long timeout is fine

        private readonly string _scriptName;
        private readonly string _watchMessage;

        public EnvisiaNodeScriptRunner(
            string scriptName,
            string watchMessage,
            IOptions<SpaOptions> optionsProvider,
            IOptions<EvSpaOptions> envisiaOptionsProvider,
            IHostApplicationLifetime applicationLifetime,
            ILoggerFactory loggerFactory, DiagnosticSource diagnosticSource, 
            EnvisiaNodeBlocker envisiaNodeBlocker)
        {
            _optionsProvider = optionsProvider;
            _envisiaOptionsProvider = envisiaOptionsProvider;
            _applicationLifetime = applicationLifetime;
            _loggerFactory = loggerFactory;
            _diagnosticSource = diagnosticSource;
            _envisiaNodeBlocker = envisiaNodeBlocker;
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

            var envisiaOptions = new EvSpaOptions
            {
                PackageManagerScript = _envisiaOptionsProvider.Value.PackageManagerScript,
            };

            var pkgManagerCommand = options.PackageManagerCommand;
            var pkgManagerScript = envisiaOptions.PackageManagerScript;
            var sourcePath = options.SourcePath;

            var logger = _loggerFactory.CreateLogger(LogCategoryName);

            var envVars = new Dictionary<string, string>();
            var scriptRunner = new NodeScriptRunner(
                sourcePath,
                _scriptName,
                null,
                envVars,
                pkgManagerCommand,
                pkgManagerScript,
                _diagnosticSource,
                _applicationLifetime.ApplicationStopping);
            scriptRunner.AttachToLogger(logger);

            using (var stdErrReader = new EventedStreamStringReader(scriptRunner.StdErr))
            {
                try
                {
                    // Although the React dev server may eventually tell us the URL it's listening on,
                    // it doesn't do so until it's finished compiling, and even then only if there were
                    // no compiler warnings. So instead of waiting for that, consider it ready as soon
                    // as it starts listening for requests.
                    await scriptRunner.StdOut.WaitForMatch(
                        new Regex(_watchMessage, RegexOptions.None, RegexMatchTimeout));
                    _envisiaNodeBlocker.CompletionSource.SetResult(true);
                }
                catch (EndOfStreamException ex)
                {
                    throw new InvalidOperationException(
                        $"The {pkgManagerCommand} script '{_scriptName}' exited without indicating that the " +
                        $"watch mode was running. The error output was: " +
                        $"{stdErrReader.ReadAsString()}", ex);
                }
            }

            await Task.CompletedTask;
        }
    }
}