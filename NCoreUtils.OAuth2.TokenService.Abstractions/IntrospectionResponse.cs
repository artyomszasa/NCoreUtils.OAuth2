using System;

namespace NCoreUtils.OAuth2
{
    public class IntrospectionResponse : IEquatable<IntrospectionResponse>
    {
        public static IntrospectionResponse Inactive { get; } = new IntrospectionResponse(
            false,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default
        );

        public bool Active { get; }

        public ScopeCollection Scope { get; }

        public string? ClientId { get; }

        public string? Email { get; }

        public string? Username { get; }

        public string? TokenType { get; }

        /// <summary>
        /// Timestamp indicating when this token will expire.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; }

        /// <summary>
        /// Timestamp indicating when this token was originally issued.
        /// </summary>
        public DateTimeOffset? IssuedAt { get; }

        /// <summary>
        /// Timestamp indicating when this token is not to be used before.
        /// </summary>
        public DateTimeOffset? NotBefore { get; }

        public string? Sub { get; }

        public string? Issuer { get; }

        public IntrospectionResponse(
            bool active,
            ScopeCollection scope,
            string? clientId,
            string? email,
            string? username,
            string? tokenType,
            DateTimeOffset? expiresAt,
            DateTimeOffset? issuedAt,
            DateTimeOffset? notBefore,
            string? sub,
            string? issuer)
        {
            Active = active;
            Scope = scope;
            ClientId = clientId;
            Email = email;
            Username = username;
            TokenType = tokenType;
            ExpiresAt = expiresAt;
            IssuedAt = issuedAt;
            NotBefore = notBefore;
            Sub = sub;
            Issuer = issuer;
        }

        public bool Equals(IntrospectionResponse other)
            => other != null
                && Active == other.Active
                && Scope == other.Scope
                && ClientId == other.ClientId
                && Email == other.Email
                && Username == other.Username
                && TokenType == other.TokenType
                && ExpiresAt == other.ExpiresAt
                && IssuedAt == other.IssuedAt
                && NotBefore == other.NotBefore
                && Sub == other.Sub
                && Issuer == other.Issuer;

        public override bool Equals(object? obj)
            => obj is IntrospectionResponse other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(
                HashCode.Combine(Active, Scope, ClientId, Email, Username, TokenType),
                HashCode.Combine(ExpiresAt, IssuedAt, NotBefore, Sub, Issuer)
            );
    }
}