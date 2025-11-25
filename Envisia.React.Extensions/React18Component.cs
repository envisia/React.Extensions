using System.IO;
using React;

namespace Envisia.React.Extensions;

#nullable enable

public sealed class React18Component(
    IReactEnvironment environment,
    IReactSiteConfiguration configuration,
    IReactIdGenerator reactIdGenerator,
    string componentName,
    string containerId)
    : ReactComponent(environment, configuration, reactIdGenerator, componentName, containerId)
{
    /// <summary>
    /// Renders the JavaScript required to initialise this component client-side. This will
    /// initialise the React component, which includes attach event handlers to the
    /// server-rendered HTML.
    /// </summary>
    /// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to which the content is written</param>
    /// <param name="waitForDomContentLoad">Delays the component init until the page load event fires. Useful if the component script tags are located after the call to Html.ReactWithInit. </param>
    /// <returns>JavaScript</returns>
    public override void RenderJavaScript(TextWriter writer, bool waitForDomContentLoad)
    {
        if (waitForDomContentLoad)
        {
            writer.Write("window.addEventListener('DOMContentLoaded', function() {");
        }

        var hydrate = _configuration.UseServerSideRendering && !ClientOnly;
        writer.Write(
            !hydrate ? "ReactDOM.createRoot(" : "ReactDOM.hydrateRoot(");
        writer.Write("document.getElementById(\"");
        writer.Write(ContainerId);
        if (hydrate)
        {
            writer.Write("\")");
            writer.Write(", ");
        }
        else
        {
            writer.Write("\"))");
            writer.Write(".render(");
        }

        WriteComponentInitialiser(writer);
        writer.Write(")");

        if (waitForDomContentLoad)
        {
            writer.Write("});");
        }
    }
}