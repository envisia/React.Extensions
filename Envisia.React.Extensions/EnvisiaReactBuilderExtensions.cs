using Microsoft.AspNetCore.Builder;
using React.AspNet;

namespace Envisia.React.Extensions
{
    public static class EnvisiaReactBuilderExtensions
    {
        public static IApplicationBuilder UseReactCore(
            this IApplicationBuilder app,
            BabelFileOptions fileOptions = null)
        {
            // Allow serving of .jsx files
            app.UseMiddleware<BabelFileMiddleware>(fileOptions ?? new BabelFileOptions());

            return app;
        }
    }
}