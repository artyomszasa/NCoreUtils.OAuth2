using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCoreUtils.Logging;
using NCoreUtils.OAuth2;

namespace NCoreUtils.AspNetCore.OAuth2;

public class Program
{
    private static string GetEnvironmentName() => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") switch
    {
        null or "" => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") switch
        {
            null or "" => "Development",
            string dotnetEnv => dotnetEnv
        },
        string aspNetCoreEnv => aspNetCoreEnv
    };

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
            .AddEnvironmentVariables("OAUTH2_")
            .Build();

    private static void ConfigureLogging(ILoggingBuilder builder, IConfiguration configuration, IWebHostEnvironment environment)
    {
        builder
            .ClearProviders()
            .AddConfiguration(configuration.GetSection("Logging"));
#if DEBUG
        if (environment.IsDevelopment())
        {
            builder.AddConsole().AddDebug();
        }
        else
        {
#endif
            builder.AddGoogleFluentd<AspNetCoreLoggerProvider>(projectId: configuration["Google:ProjectId"]);
#if DEBUG
        }
#endif
    }

    private static void ConfigureKestrel(KestrelServerOptions options)
        => options.Listen(GetListenEndpoint());

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            EnvironmentName = GetEnvironmentName(),
            ContentRootPath = Environment.CurrentDirectory
        });
        builder.Host.UseConsoleLifetime();
        // * CONFIGURATION *********************************************************************************************
        var configuration = CreateConfiguration();
        builder.Configuration.AddConfiguration(configuration);
        // * LOGGING ***************************************************************************************************
        ConfigureLogging(builder.Logging, configuration, builder.Environment);
        // * KESTREL ***************************************************************************************************
        builder.WebHost.UseKestrel(ConfigureKestrel);
        // * SERVICES **************************************************************************************************
        var providers = configuration.GetSection("LoginProviders").GetLoginProviderConfigurations();
        var tokenServiceConfiguration = configuration.GetSection("TokenService").GetTokenServiceConfiguration();
        var aesConfiguration = configuration.GetSection("Aes").GetAesTokenEncryptionConfiguration();
        builder.Services
            // client pooling
            .AddHttpClient()
            // http context accessor
            .AddHttpContextAccessor()
            // token service
            .AddSingleton(aesConfiguration)
            .AddFirestoreTokenRepository(configuration["Google:ProjectId"])
            .AddTokenService<AesTokenEncryption, FirestoreTokenRepository>(tokenServiceConfiguration)
            // scoped login provider client
            .AddDynamicLoginProvider(providers)
            // CORS
            .AddCors(b => b.AddDefaultPolicy(opts => opts
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                // must be at least 2 domains for CORS middleware to send Vary: Origin
                .WithOrigins("https://example.com", "http://127.0.0.1")
                .SetIsOriginAllowed(_ => true)
            ))
            // routing
            .AddRouting();
        // * BUILD *****************************************************************************************************
        var app = builder.Build();
        // * CONFIGURE *************************************************************************************************
        app
            .UseForwardedHeaders(configuration.GetSection("ForwardedHeaders"))
            .UseCors()
            .UseRouting()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapGet("healthz", context => { context.Response.StatusCode = 200; return Task.CompletedTask; });
                endpoints.MapTokenService(string.Empty);
            });
        // * RUN *******************************************************************************************************
        app.Start();
        app.WaitForShutdown();
    }
}