using System;
using OpenTelemetry.Trace;

namespace Envisia.React.Extensions;

public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Subscribes to the EnvisiaReact activity source to enable OpenTelemetry tracing.
    /// </summary>
    public static TracerProviderBuilder AddEnvisiaReact(this TracerProviderBuilder builder)
        => builder.AddSource(Telementry.ServiceName);
}