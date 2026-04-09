using Npgsql;
using static NCoreUtils.OAuth2.ServiceCollectionNpgsqlTokenRepositoryExtensions;

namespace NCoreUtils.OAuth2;

public class NpgsqlTokenRepository(TokenRepositoryNpgsqlDataSource dataSource, NpgsqlTokenRepositoryConfiguration configuration) : ITokenRepository
{
    private readonly string InsertCommandText = $@"INSERT INTO ""{configuration.RefreshTokenTableName}""
    (sub, issuer, email, username, scopes, issued_at, expires_at)
    VALUES($1, $2, $3, $4, $5, $6, $7);";

    private readonly string CheckCommandText = @$"SELECT COUNT(1)::INT FROM ""{configuration.RefreshTokenTableName}""
    WHERE sub = $1 AND issued_at = $2 AND scopes = $3;";

    private readonly NpgsqlDataSource DataSource = (dataSource ?? throw new ArgumentNullException(nameof(dataSource))).DataSource;

    public async ValueTask<bool> CheckRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
    {
        // it order to perform SQL-side checks scopes must be ordered
        var scopes = token.Scopes.ToArray();
        Array.Sort(scopes, StringComparer.InvariantCultureIgnoreCase);
        await using var command = DataSource.CreateCommand(CheckCommandText);
        command.Parameters.AddWithValue(token.Sub);
        command.Parameters.AddWithValue(token.IssuedAt.UtcTicks);
        command.Parameters.AddWithValue(scopes);
        var count = await command.ExecuteScalarAsync(cancellationToken) switch
        {
            null => 0,
            int i => i,
            var obj => throw new InvalidOperationException($"Check command returned object of type {obj.GetType()}")
        };
        return count > 0;
    }

    public async ValueTask PersistRefreshTokenAsync(Token token, CancellationToken cancellationToken = default)
    {
        // it order to perform SQL-side checks scopes must be ordered
        var scopes = token.Scopes.ToArray();
        Array.Sort(scopes, StringComparer.InvariantCultureIgnoreCase);
        await using var command = DataSource.CreateCommand(InsertCommandText);
        command.Parameters.AddWithValue(token.Sub);
        command.Parameters.AddWithValue(token.Issuer);
        command.Parameters.AddWithValue(token.Email!);
        command.Parameters.AddWithValue(token.Username);
        command.Parameters.AddWithValue(scopes);
        command.Parameters.AddWithValue(token.IssuedAt.UtcTicks);
        command.Parameters.AddWithValue(token.ExpiresAt.UtcTicks);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}