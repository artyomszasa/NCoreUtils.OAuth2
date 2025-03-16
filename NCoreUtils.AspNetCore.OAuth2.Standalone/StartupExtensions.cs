using System.Globalization;
using NCoreUtils.OAuth2;

namespace NCoreUtils.AspNetCore.OAuth2;

internal static class StartupExtensions
{
    private static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            var path = configuration is IConfigurationSection section ? $"{section.Path}:{key}" : key;
            throw new InvalidOperationException($"No required value found at {path}");
        }
        return value;
    }

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
                        Endpoint = configuration.EndpointOrigin,
                        HttpClient = configuration.HttpClient,
                        Path = configuration.EndpointPath
                    }, serviceProvider.GetRequiredService<IHttpClientFactory>());
                }
                throw new InvalidOperationException($"No configuration found for host {httpContext.Request.Host}.");
            });
    }

    public static List<LoginProviderConfiguration> GetLoginProviderConfigurations(this IConfiguration configuration)
    {
        var configurations = new List<LoginProviderConfiguration>();
        foreach (var section in configuration.GetChildren())
        {
            var host = section[nameof(LoginProviderConfiguration.Host)];
            IReadOnlyList<string> hosts;
            var hostsSection = configuration.GetSection(nameof(LoginProviderConfiguration.Hosts));
            if (hostsSection.Exists())
            {
                hosts = hostsSection.AsEnumerable()
                    .Select(static kv => kv.Value)
                    .Where(static v => !string.IsNullOrEmpty(v))
                    .ToArray()!;
            }
            else
            {
                hosts = [];
            }
            var httpClient = section[nameof(LoginProviderConfiguration.HttpClient)] ?? LoginProviderConfiguration.DefaultHttpClientConfigurationName;
            var endpoint = section[nameof(LoginProviderConfiguration.Endpoint)]
                ?? throw new InvalidOperationException($"Host property is required at {section.Path}.");
            configurations.Add(new(host, hosts, httpClient, endpoint));
        }
        return configurations;
    }

    public static TokenServiceConfiguration GetTokenServiceConfiguration(this IConfiguration configuration)
    {
        var rawRefreshTokenExpiry = configuration[nameof(TokenServiceConfiguration.RefreshTokenExpiry)];
        TimeSpan refreshTokenExpiry;
        if (rawRefreshTokenExpiry is null)
        {
            refreshTokenExpiry = TokenServiceConfiguration.DefaultRefreshTokenExpiry;
        }
        else if (!TimeSpan.TryParse(rawRefreshTokenExpiry, CultureInfo.InvariantCulture, out refreshTokenExpiry))
        {
            throw new InvalidOperationException($"Invalid {nameof(TokenServiceConfiguration.RefreshTokenExpiry)} value at {(configuration as IConfigurationSection)?.Path}.");
        }
        var rawAccessTokenExpiry = configuration[nameof(TokenServiceConfiguration.AccessTokenExpiry)];
        TimeSpan accessTokenExpiry;
        if (rawAccessTokenExpiry is null)
        {
            accessTokenExpiry = TokenServiceConfiguration.DefaultAccessTokenExpiry;
        }
        else if (!TimeSpan.TryParse(rawAccessTokenExpiry, CultureInfo.InvariantCulture, out accessTokenExpiry))
        {
            throw new InvalidOperationException($"Invalid {nameof(TokenServiceConfiguration.AccessTokenExpiry)} value at {(configuration as IConfigurationSection)?.Path}.");
        }
        return new(refreshTokenExpiry, accessTokenExpiry);
    }

    public static AesTokenEncryptionConfiguration GetAesTokenEncryptionConfiguration(this IConfiguration configuration) => new()
    {
        Key = configuration.GetRequiredValue(nameof(AesTokenEncryptionConfiguration.Key)),
        IV = configuration.GetRequiredValue(nameof(AesTokenEncryptionConfiguration.IV)),
    };
}