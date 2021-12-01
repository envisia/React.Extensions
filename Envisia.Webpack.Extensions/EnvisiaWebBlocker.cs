using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Envisia.Webpack.Extensions
{
    public class EnvisiaWebBlocker
    {
        public void WaitForNodeCompilation(IApplicationBuilder app, bool condition = true)
        {
            if (condition)
            {
                var nodeBlocker = app.ApplicationServices.GetRequiredService<EnvisiaNodeBlocker>();
                app.Use(async (context, func) =>
                {
                    await nodeBlocker.CompletionSource.Task;
                    await func();
                });
            }
        }
    }
}