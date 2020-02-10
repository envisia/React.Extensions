using System;
using System.IO;
using Microsoft.AspNetCore.Razor.TagHelpers;
using React;

namespace Envisia.React.Extensions
{
    public abstract class EnvisiaReactSharedTagHelper : TagHelper
    {
        protected readonly IReactEnvironment ReactEnvironment;

        protected EnvisiaReactSharedTagHelper(IReactEnvironment reactEnvironment)
        {
            ReactEnvironment = reactEnvironment;
        }

        protected void WriteScriptTag(TagHelperOutput output)
        {
            output.TagName = "script";
            if (ReactEnvironment.Configuration.ScriptNonceProvider != null)
            {
                output.Attributes.Add("nonce", ReactEnvironment.Configuration.ScriptNonceProvider());
            }
        }

        protected void WriteScriptTag(TextWriter writer, Action<TextWriter> bodyWriter)
        {
            writer.Write("<script");
            if (ReactEnvironment.Configuration.ScriptNonceProvider != null)
            {
                writer.Write(" nonce=\"");
                writer.Write(ReactEnvironment.Configuration.ScriptNonceProvider());
                writer.Write("\"");
            }

            writer.Write(">");

            bodyWriter(writer);

            writer.Write("</script>");
        }
    }
}