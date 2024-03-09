using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2;

public static class ServiceCollectionTokenServiceClientExtensions
{
    public static IServiceCollection AddTokenServiceClient(this IServiceCollection services, TokenServiceEndpointsClientConfiguration configuration, string? path = null)
        => services
            .AddTokenServiceEndpointsClient(configuration, path)
            .AddSingleton<ITokenService, TokenServiceEndpointsWrapper>();

    public static IServiceCollection AddTokenServiceClient(this IServiceCollection services, string endpoint, string? httpClientConfiguration = default, string? path = default)
        => services
            .AddTokenServiceEndpointsClient(endpoint, httpClientConfiguration, path)
            .AddSingleton<ITokenService, TokenServiceEndpointsWrapper>();

    public static IServiceCollection AddTokenServiceClient(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddTokenServiceEndpointsClient(configuration)
            .AddSingleton<ITokenService, TokenServiceEndpointsWrapper>();
}