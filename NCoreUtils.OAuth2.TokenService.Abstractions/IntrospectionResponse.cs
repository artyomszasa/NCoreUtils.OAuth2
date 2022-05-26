using System;
using System.Text.Json.Serialization;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    // TODO: use record once STJ bug is resolved.
    public class IntrospectionResponse : IEquatable<IntrospectionResponse>
    {
        public static bool operator==(IntrospectionResponse? a, IntrospectionResponse? b)
        {
            if (a is null)
            {
                return b is null;
            }
            return a.Equals(b);
        }

        public static bool operator!=(IntrospectionResponse? a, IntrospectionResponse? b)
        {
            if (a is null)
            {
                return b is not null;
            }
            return !a.Equals(b);
        }

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

        [JsonPropertyName("active")]
        public bool Active { get; }

        [JsonPropertyName("scope")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public ScopeCollection Scope { get; }

        [JsonPropertyName("client_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ClientId { get; }

        [JsonPropertyName("email")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Email { get; }

        [JsonPropertyName("username")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Username { get; }

        [JsonPropertyName("token_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TokenType { get; }

        /// <summary>
        /// Timestamp indicating when this token will expire.
        /// </summary>
        [JsonPropertyName("exp")]
        [JsonConverter(typeof(DateTimeOffsetUnixTimeSecondsConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? ExpiresAt { get; }

        /// <summary>
        /// Timestamp indicating when this token was originally issued.
        /// </summary>
        [JsonPropertyName("iat")]
        [JsonConverter(typeof(DateTimeOffsetUnixTimeSecondsConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? IssuedAt { get; }

        /// <summary>
        /// Timestamp indicating when this token is not to be used before.
        /// </summary>
        [JsonPropertyName("nbf")]
        [JsonConverter(typeof(DateTimeOffsetUnixTimeSecondsConverter))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? NotBefore { get; }

        [JsonPropertyName("sub")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Sub { get; }

        [JsonPropertyName("iss")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Issuer { get; }

        [JsonConstructor]
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

        public bool Equals(IntrospectionResponse? other)
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