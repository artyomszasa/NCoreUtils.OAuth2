using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.AspNetCore;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    public static class ServiceCollectionLoginProviderClientExtensions
    {
        public static IServiceCollection AddTokenServiceClient(
            this IServiceCollection services,
            IEndpointConfiguration configuration)
            => services.AddProtoClient<ILoginProvider>(configuration, b => b.ApplyDefaultLoginProviderConfiguration());
    }
}