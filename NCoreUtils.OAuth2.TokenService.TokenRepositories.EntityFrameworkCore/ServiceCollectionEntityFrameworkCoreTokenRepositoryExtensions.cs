using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2
{
    public static class ServiceCollectionEntityFrameworkCoreTokenRepositoryExtensions
    {
        public static IServiceCollection AddEntityFrameworkCoreTokenRepository(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> configureDbContext)
        {
            return services
                .AddDbContext<RefreshTokenDbContext>(o =>
                {
                    configureDbContext?.Invoke(o);
                })
                .AddDefaultDataRepositoryContext<RefreshTokenDbContext>()
                .AddEntityFrameworkCoreDataRepository<RefreshToken, int>();
        }
    }
}