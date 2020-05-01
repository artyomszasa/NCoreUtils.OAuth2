using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.AspNetCore;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    public static class ServiceCollectionTokenServiceClientExtensions
    {
        public static IServiceCollection AddTokenServiceClient(
            this IServiceCollection services,
            IEndpointConfiguration configuration)
            => services
                .AddSingleton<ITokenService, TokenServiceEndpointsWrapper>()
                .AddProtoClient<ITokenServiceEndpoints>(
                    configuration,
                    builder =>
                    {
                        builder.ApplyDefaultTokenServiceConfiguration();
                        builder.DefaultError = new OAuth2ClientError();
                        var introspect = builder.Methods.First(m => m.Method.Name == nameof(ITokenServiceEndpoints.IntrospectAsync));
                        introspect.Input = new IntrospectionClientInput();
                    }
                );

        public static IServiceCollection AddTokenServiceClient(this IServiceCollection services, string endpoint)
        {
            var configuration = new AspNetCore.Proto.EndpointConfiguration { Endpoint = endpoint };
            return services.AddTokenServiceClient(configuration);
        }

        public static IServiceCollection AddTokenServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.Get<AspNetCore.Proto.EndpointConfiguration>()
                ?? throw new InvalidOperationException("No token service endpoint configuration found.");
            return services.AddTokenServiceClient(config);
        }
    }
}