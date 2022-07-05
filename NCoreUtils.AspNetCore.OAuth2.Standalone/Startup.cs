using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.AspNetCore.OAuth2
{
    public class Startup
    {
        static ForwardedHeadersOptions ConfigureForwardedHeaders()
        {
            var opts = new ForwardedHeadersOptions();
            opts.KnownNetworks.Clear();
            opts.KnownProxies.Clear();
            opts.ForwardedHeaders = ForwardedHeaders.All;
            return opts;
        }

#pragma warning disable IDE0052
        private IWebHostEnvironment Env { get; }
#pragma warning restore IDE0052

        private IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Env = env ?? throw new ArgumentNullException(nameof(env));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode",
              Justification = "Configuration types are preserved through dynamic dependency.")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(LoginProviderConfiguration))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TokenServiceConfiguration))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AesTokenEncryptionConfiguration))]
        public void ConfigureServices(IServiceCollection services)
        {
            var providers = Configuration.GetSection("LoginProviders")
                .Get<List<LoginProviderConfiguration>>()
                ?? throw new InvalidOperationException("No login provider configuration present.");

            var tokenServiceConfiguration = Configuration.GetSection("TokenService")
                .Get<TokenServiceConfiguration>()
                ?? throw new InvalidOperationException("No token service configuration present.");

            var aesConfiguration = Configuration.GetSection("Aes")
                .Get<AesTokenEncryptionConfiguration>()
                ?? throw new InvalidOperationException("No aes configuration present.");

            services
                // client pooling
                .AddHttpClient()
                // http context accessor
                .AddHttpContextAccessor()
                // token service
                .AddSingleton(aesConfiguration)
                .AddFirestoreTokenRepository(Configuration["Google:ProjectId"])
                .AddTokenService<AesTokenEncryption, FirestoreTokenRepository>(tokenServiceConfiguration)
                // scoped login provider client
                // .AddScopedProtoClient<ILoginProvider>(
                //     serviceProvider =>
                //     {
                //         var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>()
                //             .HttpContext
                //             ?? throw new InvalidOperationException("Unable to get http context.");
                //         if (providers.TryChoose(httpContext, out var provider))
                //         {
                //             return provider;
                //         }
                //         throw new InvalidOperationException($"No configuration found for host {httpContext.Request.Host}.");
                //     },
                //     b => b.ApplyDefaultLoginProviderConfiguration()
                // )
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
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("healthz", context => { context.Response.StatusCode = 200; return Task.CompletedTask; });
                    endpoints.MapTokenService(string.Empty);
                });
        }
    }
}
