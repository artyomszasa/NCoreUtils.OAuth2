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

    public static IServiceCollection AddFirestoreTokenRepository(
        this IServiceCollection services,
        string? projectId = default,
        NCoreUtils.Data.Google.Cloud.Firestore.FirestoreConversionOptions? conversionOptions = default,
        Google.Apis.Auth.OAuth2.GoogleCredential? googleCredential = default,
        Action<Grpc.Net.Client.GrpcChannelOptions>? configureGrpcChannelOptions = default)
        => services.AddFirestoreTokenRepository(new FirestoreConfiguration
        {
            ProjectId = projectId,
            ConversionOptions = conversionOptions,
            GoogleCredential = googleCredential,
            ConfigureGrpcChannelOptions = configureGrpcChannelOptions
        });

    public static IServiceCollection AddFirestoreTokenRepository(this IServiceCollection services, string? projectId)
        => services.AddFirestoreTokenRepository(projectId, default, default, default);
}