using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NCoreUtils.AspNetCore.Rest;
using NCoreUtils.Data;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Data;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.AspNetCore.OAuth2
{
    public class Startup
    {
        private static ForwardedHeadersOptions ConfigureForwardedHeaders()
        {
            var opts = new ForwardedHeadersOptions();
            opts.KnownNetworks.Clear();
            opts.KnownProxies.Clear();
            opts.ForwardedHeaders = ForwardedHeaders.All;
            opts.ForwardLimit = 8;
            return opts;
        }

        private static void ConfigureRest(RestConfigurationBuilder builder)
        {
            builder.AddEntity<RefreshToken>();
            builder.ConfigureAccess(o => o.RestrictAll(user => user.Identity.IsAuthenticated && (user.IsInRole("admin") || user.IsInRole("website"))));
        }

        private readonly IWebHostEnvironment _env;

        private readonly IConfiguration _configuration;

        private void ConfigureDbContext(DbContextOptionsBuilder builder)
        {
            string connectionString;
            switch (Environment.GetEnvironmentVariable("NPGSQL_Config"))
            {
                case "env":
                    var pgsqlConfiguration = new ConfigurationBuilder()
                        .AddEnvironmentVariables(prefix: "PGSQL_")
                        .Build();
                    // default values
                    var connectionStringBuilder = new Npgsql.NpgsqlConnectionStringBuilder
                    {
                        Pooling = true,
                        MinPoolSize = 1,
                        PersistSecurityInfo = true,
                        LogParameters = _env.IsDevelopment()
                    };
                    // configured values
                    pgsqlConfiguration.Bind(connectionStringBuilder);
                    connectionString = connectionStringBuilder.ToString();
                    break;
                default:
                    connectionString = _configuration.GetConnectionString("Default");
                    break;

            }

            builder
                    .UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name))
                    .EnableDetailedErrors(true)
                    .EnableSensitiveDataLogging(_env.IsDevelopment());
        }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var providers = _configuration.GetSection("LoginProviders")
                .Get<List<LoginProviderConfiguration>>()
                ?? throw new InvalidOperationException("No login provider configuration present.");

            var tokenServiceConfiguration = _configuration.GetSection("TokenService")
                .Get<TokenServiceConfiguration>()
                ?? throw new InvalidOperationException("No token service configuration present.");

            var aesConfiguration = _configuration.GetSection("Aes")
                .Get<RijndaelTokenEncryptionConfiguration>()
                ?? throw new InvalidOperationException("No aes configuration present.");

            services
                // client pooling
                .AddHttpClient()
                // http context accessor
                .AddHttpContextAccessor()
                // token service
                .AddSingleton(aesConfiguration)
                .AddEntityFrameworkCoreTokenRepository(ConfigureDbContext)
                .AddTokenService<RijndaelTokenEncryption, EntityFrameworkCoreTokenRepository>(tokenServiceConfiguration)
                // scoped login provider client
                .AddScopedProtoClient<ILoginProvider>(
                    serviceProvider =>
                    {
                        var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>()
                            .HttpContext
                            ?? throw new InvalidOperationException("Unable to get http context.");
                        if (providers.TryChoose(httpContext, out var provider))
                        {
                            return provider;
                        }
                        throw new InvalidOperationException($"No configuration found for host {httpContext.Request.Host}.");
                    },
                    b => b.ApplyDefaultLoginProviderConfiguration()
                )
                // DATA query for REST
                .AddDataQueryServices(_ => {})
                // JSON options for REST requests
                .AddTransient<JsonSerializerOptions>(serviceProvider => serviceProvider.GetRequiredService<IOptionsMonitor<JsonSerializerOptions>>().CurrentValue)
                // Authorization for REST requests
                .AddScoped<ITokenHandler, BearerTokenHandler>()
                .AddAuthentication(OAuth2AuthenticationSchemeOptions.Name)
                    .AddScheme<OAuth2AuthenticationSchemeOptions, OAuth2AuthenticationHandler>(OAuth2AuthenticationSchemeOptions.Name, _ => { })
                    .Services
                .AddAuthorization()
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #if DEBUG
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            #endif

            app
                .UseForwardedHeaders(ConfigureForwardedHeaders())
                .UseCors()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapTokenService(string.Empty);
                    endpoints.MapRest("data", ConfigureRest);
                });
        }
    }
}
