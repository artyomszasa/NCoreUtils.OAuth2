using System;
using System.Text.Json.Serialization;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    // TODO: use record once STJ bug is resolved.
    public class AccessTokenResponse : IEquatable<AccessTokenResponse>
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
        public string AccessToken { get; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; }

        [JsonPropertyName("expires_in")]
        [JsonConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan ExpiresIn { get; }

        [JsonPropertyName("refresh_token")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshToken { get; }

        [JsonPropertyName("scope")]
        [JsonConverter(typeof(ScopeCollectionConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ScopeCollection Scope { get; }

        [JsonConstructor]
        public AccessTokenResponse(string accessToken, string? tokenType, TimeSpan expiresIn, string? refreshToken, ScopeCollection scope)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
            RefreshToken = refreshToken;
            Scope = scope;
        }

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
}