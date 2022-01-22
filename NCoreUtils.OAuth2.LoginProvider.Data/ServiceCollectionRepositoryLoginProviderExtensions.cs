using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils
{
    public static class ServiceCollectionRepositoryLoginProviderExtensions
    {
        private class LoginProviderConfiguration : ILoginProviderConfiguration
        {
            public string Issuer { get; set; } = default!;

            public bool UseEmailAsUsername { get; set; } = false;
        }

        public static IServiceCollection AddRepositoryLoginProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TProvider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TUser, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TId>(
            this IServiceCollection services,
            ILoginProviderConfiguration configuration)
            where TProvider : RepositoryLoginProvider<TUser, TId>
            where TUser : ILocalUser<TId>
            where TId : IConvertible
            => services
                .AddSingleton(configuration)
                .AddScoped<ILoginProvider, TProvider>();

        public static IServiceCollection AddRepositoryLoginProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TUser, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TId>(this IServiceCollection services, ILoginProviderConfiguration configuration)
            where TUser : ILocalUser<TId>
            where TId : IConvertible
            => services
                .AddSingleton(configuration)
                .AddScoped<ILoginProvider, RepositoryLoginProvider<TUser, TId>>();

        public static IServiceCollection AddRepositoryLoginProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TProvider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TUser, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TId>(
            this IServiceCollection services,
            string issuer,
            bool useEmailAsUsername = false)
            where TProvider : RepositoryLoginProvider<TUser, TId>
            where TUser : ILocalUser<TId>
            where TId : IConvertible
            => services.AddRepositoryLoginProvider<TProvider, TUser, TId>(new LoginProviderConfiguration {
                Issuer = issuer,
                UseEmailAsUsername = useEmailAsUsername
            });

        public static IServiceCollection AddRepositoryLoginProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TUser, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TId>(
            this IServiceCollection services,
            string issuer,
            bool useEmailAsUsername = false)
            where TUser : ILocalUser<TId>
            where TId : IConvertible
            => services.AddRepositoryLoginProvider<TUser, TId>(new LoginProviderConfiguration {
                Issuer = issuer,
                UseEmailAsUsername = useEmailAsUsername
            });

    }
}