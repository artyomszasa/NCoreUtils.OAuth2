using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    public static class ServiceCollectionTokenServiceExtensions
    {
        public static IServiceCollection AddTokenService<TTokenEncryption, TTokenRepository>(
            this IServiceCollection services,
            ITokenServiceConfiguration configuration,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
            ServiceLifetime tokenEncryptionLifetime = ServiceLifetime.Singleton,
            ServiceLifetime tokenRepositoryLifetime = ServiceLifetime.Scoped)
            where TTokenEncryption : class, ITokenEncryption
            where TTokenRepository : class, ITokenRepository
        {
            services.AddSingleton(configuration);
            services.Add(ServiceDescriptor.Describe(typeof(LazyService<ITokenRepository>), typeof(LazyService<ITokenRepository>), tokenRepositoryLifetime));
            services.Add(ServiceDescriptor.Describe(typeof(ITokenEncryption), typeof(TTokenEncryption), tokenEncryptionLifetime));
            services.Add(ServiceDescriptor.Describe(typeof(ITokenRepository), typeof(TTokenRepository), tokenRepositoryLifetime));
            services.Add(ServiceDescriptor.Describe(typeof(ITokenService), typeof(TokenService), serviceLifetime));
            return services;
        }
    }
}