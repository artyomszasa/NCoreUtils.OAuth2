using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.AspNetCore
{
    public static class EndpointBuilderTokenServiceExtensions
    {
        public static IEndpointConventionBuilder MapTokenService(this IEndpointRouteBuilder builder)
            => builder.MapProto<ITokenServiceEndpoints>(
                build: b =>
                {
                    b.ApplyDefaultTokenServiceConfiguration();
                    var introspect = b.Methods.First(m => m.Method.Name == nameof(ITokenServiceEndpoints.IntrospectAsync));
                    introspect.Input = new IntrospectionServiceInput();
                },
                implementationFactory: serviceProvider => new TokenServiceWrapper(serviceProvider.GetRequiredService<ITokenService>())
            );
    }
}