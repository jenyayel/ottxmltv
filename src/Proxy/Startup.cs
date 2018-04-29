using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proxy.Ott;
using System;
using System.Text;

namespace Proxy
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<OttClient>();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                if (context.Request.Path.Value != "/")
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                using (var scope = serviceProvider.CreateScope())
                {

                    try
                    {
                        var channels = await scope.ServiceProvider.GetService<OttClient>().GetChannels();

                        context.Response.Headers.Add("content-type", "application/xml");
                        using (var writer = new XmlTvWriter(context.Response.Body))
                        {
                            await writer.Write(channels);
                            await writer.Write(scope.ServiceProvider.GetService<OttClient>().GetEpg(channels));
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = 400;
                        var buffer = Encoding.UTF8.GetBytes(ex.Message);
                        await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                        return;
                    }
                }
            });
        }
    }
}
