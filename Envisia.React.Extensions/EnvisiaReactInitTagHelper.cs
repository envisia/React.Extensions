using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using React;

namespace Envisia.React.Extensions
{
    [HtmlTargetElement("react-net-init-module")]
    public class EnvisiaReactInitTagHelper : EnvisiaReactSharedTagHelper
    {
        public bool ClientOnly { get; set; } = false;

        public EnvisiaReactInitTagHelper(IReactEnvironment reactEnvironment) : base(reactEnvironment)
        {
        }
        
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            WriteScriptTag(output);
            
            using var writer = new StringWriter();
            ReactEnvironment.GetInitJavaScript(writer, ClientOnly);
            output.Content.SetHtmlContent(writer.ToString());
        }
    }
}