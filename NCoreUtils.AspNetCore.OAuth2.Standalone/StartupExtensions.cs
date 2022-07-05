using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.OAuth2;

namespace NCoreUtils.AspNetCore.OAuth2;

internal static class StartupExtensions
{
    public static IServiceCollection AddDynamicLoginProvider(this IServiceCollection services, List<LoginProviderConfiguration> configurations)
    {
        return services
            .AddScoped<ILoginProvider>(serviceProvider =>
            {
                var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext
                    ?? throw new InvalidOperationException("Unable to get http context.");
                if (configurations.TryChoose(httpContext, out var configuration))
                {
                    return new LoginProviderClient(new LoginProviderClientConfiguration
                    {
                        Endpoint = configuration.Endpoint,
                        HttpClient = configuration.HttpClient
                    }, serviceProvider.GetRequiredService<IHttpClientFactory>());
                }
                throw new InvalidOperationException($"No configuration found for host {httpContext.Request.Host}.");
            });
    }
}