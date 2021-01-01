using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCoreUtils.Logging;

namespace NCoreUtils.AspNetCore.OAuth2
{
    public class Program
    {
        private static IPEndPoint ParseEndpoint(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new IPEndPoint(IPAddress.Loopback, 5000);
            }
            var portIndex = input.LastIndexOf(':');
            if (-1 == portIndex)
            {
                return new IPEndPoint(IPAddress.Parse(input), 5000);
            }
            else
            {
                return new IPEndPoint(IPAddress.Parse(input.Substring(0, portIndex)), int.Parse(input.Substring(portIndex + 1)));
            }
        }

        /// <summary>
        /// Listening ip/port is determined as follows:
        /// <para>
        /// - if <c>PORT</c> environment variable is set --> listen at <c>0.0.0.0:{PORT}</c>;
        /// </para>
        /// <para>
        /// - if <c>ASPNETCORE_LISTEN_AT</c> environment variable is set --> listen at <c>{ASPNETCORE_LISTEN_AT}</c>;
        /// </para>
        /// <para>
        /// - otherwise listen at <c>127.0.0.1:5000</c>.
        /// </para>
        /// </summary>
        private static IPEndPoint GetListenEndpoint()
            => Environment.GetEnvironmentVariable("PORT") switch
            {
                null => ParseEndpoint(Environment.GetEnvironmentVariable("ASPNETCORE_LISTEN_AT")),
                var rawPort => new IPEndPoint(IPAddress.Any, int.Parse(rawPort))
            };

        private static IConfiguration CreateConfiguration()
            => new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("secrets/appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("OAUTH2")
                .Build();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = CreateConfiguration();
            return new HostBuilder()
                .UseContentRoot(Environment.CurrentDirectory)
                .ConfigureLogging((context, builder) =>
                {
                    builder
                        .ClearProviders()
                        .AddConfiguration(configuration.GetSection("Logging"));
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddConsole().AddDebug();
                    }
                    else
                    {
                        builder.AddGoogleFluentd<AspNetCoreLoggerProvider>(projectId: configuration["Google:ProjectId"], configureOptions: o =>
                        {
                            o.Configuration.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        });
                    }
                })
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseConfiguration(configuration)
                        .UseStartup<Startup>()
                        .UseKestrel(opts => opts.Listen(GetListenEndpoint()));
                });
        }
    }
}
