using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if GOOGLE_FLUENTD_LOGGING
using NCoreUtils.Logging;
#endif

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
                return new IPEndPoint(IPAddress.Parse(input.AsSpan(0, portIndex)), int.Parse(input.AsSpan()[(portIndex + 1)..]));
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

#pragma warning disable IDE0060
        public static IHostBuilder CreateHostBuilder(string[] args)
#pragma warning restore IDE0060
        {
            var configuration = CreateConfiguration();
            return new HostBuilder()
                .UseContentRoot(Environment.CurrentDirectory)
                .ConfigureLogging((context, builder) =>
                {
                    builder
                        .ClearProviders()
                        .AddConfiguration(configuration.GetSection("Logging"));
#if GOOGLE_FLUENTD_LOGGING
                    builder.Services.AddDefaultTraceIdProvider();
#endif
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddConsole().AddDebug();
                    }
                    else
                    {
#if GOOGLE_FLUENTD_LOGGING
                        builder.Services.AddLoggingContext();
                        builder.AddGoogleFluentd<AspNetCoreLoggerProvider>(projectId: configuration["Google:ProjectId"]);
#else
                        builder.AddSimpleConsole(o => o.SingleLine = true);
#endif
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
