using System.IO;
using Microsoft.AspNetCore.Razor.TagHelpers;
using React;

namespace Envisia.React.Extensions;

[HtmlTargetElement("react-net-init-module")]
public class EnvisiaReactInitTagHelper(IReactEnvironment reactEnvironment)
    : EnvisiaReactSharedTagHelper(reactEnvironment)
{
    public bool ClientOnly { get; set; } = false;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        WriteScriptTag(output);

        using var writer = new StringWriter();
        ReactEnvironment.GetInitJavaScript(writer, ClientOnly);
        output.Content.SetHtmlContent(writer.ToString());
    }
}