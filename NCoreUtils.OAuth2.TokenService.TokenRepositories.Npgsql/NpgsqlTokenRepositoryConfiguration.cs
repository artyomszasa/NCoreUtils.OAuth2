namespace NCoreUtils.OAuth2;

public class NpgsqlTokenRepositoryConfiguration(string refreshTokenTableName)
{
    public string RefreshTokenTableName { get; } = refreshTokenTableName switch
    {
        null or "" => throw new ArgumentException($"'{nameof(refreshTokenTableName)}' must be a valid string."),
        var value => value
    };
}