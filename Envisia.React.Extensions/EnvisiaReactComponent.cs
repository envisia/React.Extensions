using System;
using System.IO;
using System.Text;
using JavaScriptEngineSwitcher.Core;
using React;

namespace Envisia.React.Extensions;

public class EnvisiaReactComponent : ReactComponent
{
    [ThreadStatic]
    private static StringWriter _staticSharedStringWriter;

    public EnvisiaReactComponent(
        IReactEnvironment environment,
        IReactSiteConfiguration configuration,
        IReactIdGenerator reactIdGenerator,
        string componentName,
        string containerId) : base(environment, configuration, reactIdGenerator, componentName, containerId)
    {
    }

    /// <summary>
    /// Renders the HTML for this component. This will execute the component server-side and
    /// return the rendered HTML.
    /// </summary>
    /// <param name="writer">The <see cref="T:System.IO.TextWriter" /> to which the content is written</param>
    /// <param name="renderContainerOnly">Only renders component container. Used for client-side only rendering.</param>
    /// <param name="renderServerOnly">Only renders the common HTML mark up and not any React specific data attributes. Used for server-side only rendering.</param>
    /// <param name="exceptionHandler">A custom exception handler that will be called if a component throws during a render. Args: (Exception ex, string componentName, string containerId)</param>
    /// <param name="renderFunctions">Functions to call during component render</param>
    /// <returns>HTML</returns>
    public override void RenderHtml(
        TextWriter writer,
        bool renderContainerOnly = false,
        bool renderServerOnly = false,
        Action<Exception, string, string> exceptionHandler = null, 
        IRenderFunctions renderFunctions = null)
    {
        if (!_configuration.UseServerSideRendering)
        {
            renderContainerOnly = true;
        }

        if (!renderContainerOnly)
        {
            EnsureComponentExists();
        }

        var html = string.Empty;
        if (!renderContainerOnly)
        {
            var stringWriter = _staticSharedStringWriter;
            if (stringWriter != null)
            {
                stringWriter.GetStringBuilder().Clear();
            }
            else
            {
                _staticSharedStringWriter = stringWriter = new StringWriter(new StringBuilder(_serializedProps.Length + 128));
            }

            try
            {
                stringWriter.Write(renderServerOnly
                    ? "ReactDOMServer.renderToStaticNodeStream("
                    : "ReactDOMServer.renderToReadableStream(");
                if (renderFunctions != null)
                {
                    stringWriter.Write(renderFunctions.WrapComponent(GetStringFromWriter2(WriteComponentInitialiser)));
                }
                else
                {
                    WriteComponentInitialiser(stringWriter);
                }

                stringWriter.Write(')');

                if (renderFunctions != null)
                {
                    renderFunctions.PreRender(x => _environment.Execute<string>(x));
                    html = _environment.Execute<string>(renderFunctions.TransformRenderedHtml(stringWriter.ToString()));
                    renderFunctions.PostRender(x => _environment.Execute<string>(x));
                }
                else
                {
                    html = _environment.Execute<string>(stringWriter.ToString());
                }

                if (renderServerOnly)
                {
                    writer.Write(html);
                    return;
                }
            }
            catch (JsException ex)
            {
                if (exceptionHandler == null)
                {
                    exceptionHandler = _configuration.ExceptionHandler;
                }

                exceptionHandler(ex, ComponentName, ContainerId);
            }
        }

        writer.Write('<');
        writer.Write(ContainerTag);
        writer.Write(" id=\"");
        writer.Write(ContainerId);
        writer.Write('"');
        if (!string.IsNullOrEmpty(ContainerClass))
        {
            writer.Write(" class=\"");
            writer.Write(ContainerClass);
            writer.Write('"');
        }

        writer.Write('>');
        writer.Write(html);
        writer.Write("</");
        writer.Write(ContainerTag);
        writer.Write('>');
    }

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
        if (hydrate)
        {
            writer.Write("ReactDOM.hydrateRoot(");
            writer.Write("document.getElementById(\"");
            writer.Write(ContainerId);
            writer.Write("\")");
            writer.Write(", ");
            WriteComponentInitialiser(writer);
            writer.Write(")");
        }
        else
        {
            writer.Write("ReactDOM.createRoot(");
            writer.Write("document.getElementById(\"");
            writer.Write(ContainerId);
            writer.Write("\"))");
            writer.Write(".render(");
            WriteComponentInitialiser(writer);
            writer.Write(")");
        }

        if (waitForDomContentLoad)
        {
            writer.Write("});");
        }
    }
    
    private string GetStringFromWriter2(Action<TextWriter> fnWithTextWriter)
    {
        using var textWriter = new StringWriter();
        fnWithTextWriter(textWriter);
        return textWriter.ToString();
    }
}