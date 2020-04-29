using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data;
using NCoreUtils.Data.Build;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2
{
    public static class ServiceCollectionFirestoreTokenRepositoryExtensions
    {
        public static IServiceCollection AddFirestoreTokenRepository(this IServiceCollection services, FirestoreConfiguration? configuration)
        {
            var modelBuilder = new DataModelBuilder();
            modelBuilder.Entity<RefreshToken>(b =>
            {
                b
                    .SetName("refreshToken")
                    .SetKey(e => e.Id);
                b.Property(e => e.Sub)
                    .SetName("sub")
                    .SetUnicode(true)
                    .SetRequired(true);
                b.Property(e => e.Issuer)
                    .SetName("issuer")
                    .SetUnicode(true)
                    .SetRequired(true);
                b.Property(e => e.Email)
                    .SetName("email")
                    .SetUnicode(true)
                    .SetRequired(false);
                b.Property(e => e.Username)
                    .SetName("username")
                    .SetUnicode(true)
                    .SetRequired(true);
                b.Property(e => e.Scopes)
                    .SetName("scopes")
                    .SetUnicode(true)
                    .SetRequired(true);
                b.Property(e => e.IssuedAt)
                    .SetName("issuedAt");
                b.Property(e => e.ExpiresAt)
                    .SetName("expiresAt");
            });
            return services
                .AddSingleton(modelBuilder)
                .AddFirestoreDataRepositoryContext(configuration ?? new FirestoreConfiguration())
                .AddFirestoreDataRepository<RefreshToken>();
        }

        public static IServiceCollection AddFirestoreTokenRepository(this IServiceCollection services, string? projectId = null)
            => services.AddFirestoreTokenRepository(new FirestoreConfiguration { ProjectId = projectId });
    }
}