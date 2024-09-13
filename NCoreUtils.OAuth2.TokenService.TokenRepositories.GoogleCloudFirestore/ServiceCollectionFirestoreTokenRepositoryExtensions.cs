using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data;
using NCoreUtils.Data.Build;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2;

public static class ServiceCollectionFirestoreTokenRepositoryExtensions
{
    public static IServiceCollection AddFirestoreTokenRepositoryWithoutModel(
        this IServiceCollection services,
        bool addRepositoryContext = false,
        FirestoreConfiguration? configuration = default)
    {
        if (addRepositoryContext)
        {
            services
                .AddFirestoreDataRepositoryContext(configuration ?? new FirestoreConfiguration());
        }
        return services.AddFirestoreDataRepository<RefreshToken>();
    }

    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Decoration only.")]
    public static IServiceCollection AddFirestoreTokenRepository(this IServiceCollection services, FirestoreConfiguration? configuration = default)
    {
        var modelBuilder = new DataModelBuilder();
        new DefaultRefreshTokenDataContext().Apply(modelBuilder);
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.SetKey(e => e.Id);
            b.Property(e => e.Sub)
                .SetUnicode(true)
                .SetRequired(true);
            b.Property(e => e.Issuer)
                .SetUnicode(true)
                .SetRequired(true);
            b.Property(e => e.Email)
                .SetUnicode(true)
                .SetRequired(false);
            b.Property(e => e.Username)
                .SetUnicode(true)
                .SetRequired(true);
            b.Property(e => e.Scopes)
                .SetUnicode(true)
                .SetRequired(true);
        });
        return services
            .AddSingleton(modelBuilder)
            .AddFirestoreTokenRepositoryWithoutModel(addRepositoryContext: true, configuration: configuration);
    }

    public static IServiceCollection AddFirestoreTokenRepository(this IServiceCollection services, string? projectId = null)
        => services.AddFirestoreTokenRepository(new FirestoreConfiguration { ProjectId = projectId });
}