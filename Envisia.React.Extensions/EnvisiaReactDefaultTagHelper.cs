using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using React;

namespace Envisia.React.Extensions;

[HtmlTargetElement("react-net-module")]
public class EnvisiaReactDefaultTagHelper(IReactEnvironment reactEnvironment)
    : EnvisiaReactSharedTagHelper(reactEnvironment)
{
    public string ComponentName { get; set; }
    public object Properties { get; set; } = new { };
    public string HtmlTag { get; set; } = null;
    public string ContainerId { get; set; } = null;
    public bool ClientOnly { get; set; } = false;
    public bool ServerOnly { get; set; } = false;
    public bool SkipLazyInit { get; set; } = false;
    public string ContainerClass { get; set; } = null;
    public Action<Exception, string, string> ExceptionHandler { get; set; } = null;
    public IRenderFunctions RenderFunctions { get; set; } = null;

    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument", Justification = "better naming")]
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        using var envisiaReactActivity = Telementry.EnvisiaReactExtensionsSource
            .StartActivity("EnvisiaReactTagActivity");
        try
        {
            if (string.IsNullOrEmpty(ComponentName))
            {
                throw new Exception("invalid component name");
            }

            var reactComponent = ReactEnvironment
                .CreateComponent(ComponentName, Properties, ContainerId, ClientOnly, ServerOnly, SkipLazyInit);
            if (!string.IsNullOrEmpty(HtmlTag))
            {
                reactComponent.ContainerTag = HtmlTag;
            }

            if (!string.IsNullOrEmpty(ContainerClass))
            {
                reactComponent.ContainerClass = ContainerClass;
            }

            output.TagName = null;

            await using var writer = new StringWriter();
            reactComponent.RenderHtml(writer, ClientOnly, ServerOnly, ExceptionHandler, RenderFunctions);
            await writer.WriteLineAsync();

            output.Content.SetHtmlContent(writer.ToString());
        }
        finally
        {
            ReactEnvironment.ReturnEngineToPool();
        }
    }
}