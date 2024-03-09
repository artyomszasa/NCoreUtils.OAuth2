using System;
using System.Text.Json.Serialization;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2;

// TODO: use record once STJ bug is resolved.
[method: JsonConstructor]
public class AccessTokenResponse(string accessToken, string? tokenType, TimeSpan expiresIn, string? refreshToken, ScopeCollection scope) : IEquatable<AccessTokenResponse>
{
    public static bool operator==(AccessTokenResponse? a, AccessTokenResponse? b)
    {
        if (a is null)
        {
            return b is null;
        }
        return a.Equals(b);
    }

    public static bool operator!=(AccessTokenResponse? a, AccessTokenResponse? b)
    {
        if (a is null)
        {
            return b is not null;
        }
        return !a.Equals(b);
    }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; } = accessToken;

    [JsonPropertyName("token_type")]
    public string? TokenType { get; } = tokenType;

    [JsonPropertyName("expires_in")]
    [JsonConverter(typeof(TimeSpanSecondsConverter))]
    public TimeSpan ExpiresIn { get; } = expiresIn;

    [JsonPropertyName("refresh_token")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RefreshToken { get; } = refreshToken;

    [JsonPropertyName("scope")]
    [JsonConverter(typeof(ScopeCollectionConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ScopeCollection Scope { get; } = scope;

    public bool Equals(AccessTokenResponse? other)
        => other != null
            && AccessToken == other.AccessToken
            && TokenType == other.TokenType
            && ExpiresIn == other.ExpiresIn
            && RefreshToken == other.RefreshToken
            && Scope == other.Scope;

    public override bool Equals(object? obj)
        => obj is AccessTokenResponse other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(AccessToken, TokenType, ExpiresIn, RefreshToken, Scope);
}