using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Envisia.Vite.Extensions
{
    public static class EnvisiaViteWebBlocker
    {
        public static void WaitForViteCompilation(this IApplicationBuilder app, bool condition = true)
        {
            if (condition)
            {
                var viteBlocker = app.ApplicationServices.GetRequiredService<EnvisiaViteBlocker>();
                app.Use(async (context, func) =>
                {
                    await viteBlocker.CompletionSource.Task;
                    await func();
                });
            }
        }
    }
}