using System;

namespace NCoreUtils.OAuth2
{
    public class AccessTokenResponse : IEquatable<AccessTokenResponse>
    {
        public string AccessToken { get; }

        public string? TokenType { get; }

        public TimeSpan ExpiresIn { get; }

        public string? RefreshToken { get; }

        public ScopeCollection Scope { get; }

        public AccessTokenResponse(string accessToken, string? tokenType, TimeSpan expiresIn, string? refreshToken, ScopeCollection scope)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
            RefreshToken = refreshToken;
            Scope = scope;
        }

        public bool Equals(AccessTokenResponse other)
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