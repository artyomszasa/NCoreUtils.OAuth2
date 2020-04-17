using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.AspNetCore
{
    public static class EndpointBuilderLoginProviderExtensions
    {
        public static IEndpointConventionBuilder MapLoginProvider(this IEndpointRouteBuilder builder, string? prefix = default)
            => builder.MapProto<ILoginProvider>(b => b.ApplyDefaultLoginProviderConfiguration(prefix));
    }
}