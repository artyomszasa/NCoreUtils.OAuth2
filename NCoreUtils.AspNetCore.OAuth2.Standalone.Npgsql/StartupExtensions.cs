using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.OAuth2;

namespace NCoreUtils.AspNetCore.OAuth2;

internal static class StartupExtensions
{
    private static IReadOnlyList<LoginProviderConfiguration> GetLoginProviderConfigurations(this IConfiguration configuration)
    {
        var configurations = new List<LoginProviderConfiguration>();
        foreach (var section in configuration.GetChildren())
        {
            var host = section[nameof(LoginProviderConfiguration.Host)];
            List<string>? hosts = default;
            foreach (var sub in section.GetSection(nameof(LoginProviderConfiguration.Hosts)).GetChildren())
            {
                if (!string.IsNullOrEmpty(sub.Value))
                {
                    (hosts ??= new()).Add(sub.Value);
                }
            }
            var httpClient = section[nameof(LoginProviderConfiguration.HttpClient)];
            var endpoint = section.GetRequiredValue(nameof(LoginProviderConfiguration.Endpoint));
            var path = section[nameof(LoginProviderConfiguration.Path)];
            configurations.Add(new(host, hosts, httpClient, endpoint, path));
        }
        return configurations;
    }

    internal static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            var path = configuration is IConfigurationSection section ? $"{section.Path}:{key}" : key;
            throw new InvalidOperationException($"No required value found at {path}");
        }
        return value;
    }

    internal static TokenServiceConfiguration GetTokenServiceConfiguration(this IConfiguration configuration) => new(
        refreshTokenExpiry: TimeSpan.Parse(configuration.GetRequiredValue(nameof(TokenServiceConfiguration.RefreshTokenExpiry))),
        accessTokenExpiry: TimeSpan.Parse(configuration.GetRequiredValue(nameof(TokenServiceConfiguration.AccessTokenExpiry)))
    );

    internal static AesTokenEncryptionConfiguration GetAesTokenEncryptionConfiguration(this IConfiguration configuration) => new()
    {
        Key = configuration.GetRequiredValue(nameof(AesTokenEncryptionConfiguration.Key)),
        IV = configuration.GetRequiredValue(nameof(AesTokenEncryptionConfiguration.IV))
    };

    public static IServiceCollection AddDynamicLoginProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var providers = configuration.GetLoginProviderConfigurations();
        return services
            .AddScoped<ILoginProvider>(serviceProvider =>
            {
                var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext
                    ?? throw new InvalidOperationException("Unable to get http context.");
                if (providers.TryChoose(httpContext, out var configuration))
                {
                    return new LoginProviderClient(new LoginProviderClientConfiguration
                    {
                        Endpoint = configuration.Endpoint,
                        HttpClient = configuration.HttpClient,
                        Path = configuration.Path
                    }, serviceProvider.GetRequiredService<IHttpClientFactory>());
                }
                throw new NoConfigurationForHostException($"No configuration found for host {httpContext.Request.Host}.");
            });
    }
}