using System.Linq;
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
                        var introspect = builder.Methods.First(m => m.Method.Name == nameof(ITokenServiceEndpoints.IntrospectAsync));
                        introspect.Input = new IntrospectionClientInput();
                    }
                );
    }
}