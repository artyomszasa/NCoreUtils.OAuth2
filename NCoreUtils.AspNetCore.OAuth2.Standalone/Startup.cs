using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.AspNetCore.OAuth2
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        private readonly IConfiguration _configuration;

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
                .AddTokenService<CompressedRijndaelTokenEncryption, X>(tokenServiceConfiguration)
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

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapTokenService(string.Empty);
            });
        }
    }
}
