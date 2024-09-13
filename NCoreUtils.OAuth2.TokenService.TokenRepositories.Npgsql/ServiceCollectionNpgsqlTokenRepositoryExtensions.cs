using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace NCoreUtils.OAuth2;

public static class ServiceCollectionNpgsqlTokenRepositoryExtensions
{
    public sealed class TokenRepositoryNpgsqlDataSource(NpgsqlDataSource dataSource)
        : IDisposable, IAsyncDisposable
    {
        public NpgsqlDataSource DataSource { get; } = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

        public void Dispose()
            => dataSource.Dispose();

        public ValueTask DisposeAsync()
            => dataSource.DisposeAsync();
    }

    public static IServiceCollection AddNpgsqlTokenRepository(
        this IServiceCollection services,
        string connectionString,
        NpgsqlTokenRepositoryConfiguration? configuration = default,
        Action<IServiceProvider, NpgsqlDataSourceBuilder>? configure = default)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        var config = configuration ?? new("refresh_token");
        if (string.IsNullOrEmpty(config.RefreshTokenTableName))
        {
            throw new InvalidOperationException("Table is not valid.");
        }
        return services
            .AddSingleton(config)
            .AddSingleton(serviceProvider =>
            {
                configure?.Invoke(serviceProvider, builder);
                return new TokenRepositoryNpgsqlDataSource(builder.Build());
            });
    }

    public static IServiceCollection AddNpgsqlTokenRepository(
        this IServiceCollection services,
        string connectionString,
        string? refreshTokenTableName)
        => services.AddNpgsqlTokenRepository(
            connectionString,
            refreshTokenTableName is null ? default : new(refreshTokenTableName),
            configure: static (serviceProvider, builder) =>
            {
                if (serviceProvider.GetService<ILoggerFactory>() is ILoggerFactory loggerFactory)
                {
                    builder.UseLoggerFactory(loggerFactory);
                }
            }
        );
}