using System;
using System.Collections.Generic;

namespace Envisia.Webpack.Extensions;

#nullable enable

public sealed class EvSpaOptions
{
    public string PackageManagerScript { get; set; } = "run";
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new ();
}