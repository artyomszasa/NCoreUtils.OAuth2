using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data;
using NCoreUtils.Data.EntityFrameworkCore;

namespace NCoreUtils.OAuth2.Data
{
    public static class ServiceCollectionOAuth2DataExtensions
    {
        static IServiceCollection AddDataRepository<TRepository, TData, TId>(this IServiceCollection services)
            where TRepository : class, IDataRepository<TData, TId>
            where TData : class, IHasId<TId>
            => services.AddScoped<TRepository>()
                .AddScoped<IDataRepository<TData, TId>>(serviceProvider => serviceProvider.GetRequiredService<TRepository>())
                .AddScoped<IDataRepository<TData>>(serviceProvider => serviceProvider.GetRequiredService<TRepository>());

        static IServiceCollection AddDataRepository<TData, TId>(this IServiceCollection services)
            where TData : class, IHasId<TId>
            where TId : IComparable<TId>
            => services.AddDataRepository<DataRepository<TData, TId>, TData, TId>();

        public static IServiceCollection AddOAuth2DbContext(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        {
            return services.AddDbContext<OAuth2DbContext>(optionsAction, contextLifetime, optionsLifetime);
        }

        public static IServiceCollection AddOAuth2DataRepositories(this IServiceCollection services)
        {
            services
                .AddDefaultDataRepositoryContext<OAuth2DbContext>()
                .AddDataRepository<AuthorizationCode, Guid>()
                .AddDataRepository<ClientApplication, int>()
                .AddDataRepository<Domain, int>()
                .AddDataRepository<Permission, int>()
                .AddDataRepository<RefreshToken, long>()
                .AddDataRepository<UserRepository, User, int>();
            return services;
        }

    }
}