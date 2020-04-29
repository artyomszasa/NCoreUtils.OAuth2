using System;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils
{
    public static class ServiceCollectionRepositoryLoginProviderExtensions
    {
        public static IServiceCollection AddRepositoryLoginProvider<TUser, TId>(this IServiceCollection services)
            where TUser : ILocalUser<TId>
            where TId : IConvertible
            => services
                .AddScoped<ILoginProvider, RepositoryLoginProvider<TUser, TId>>();
    }
}