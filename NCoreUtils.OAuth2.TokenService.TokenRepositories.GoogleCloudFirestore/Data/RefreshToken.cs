using System;
using NCoreUtils.Data;

namespace NCoreUtils.OAuth2.Data
{
    public class RefreshToken : IHasId<string>
    {
        public string Id { get; }

        public string Sub { get; }

        public string Issuer { get; }

        public string? Email { get; }

        public string Username { get; }

        public string Scopes { get; }

        public DateTimeOffset IssuedAt { get; }

        public DateTimeOffset ExpiresAt { get; }

        public RefreshToken(
            string id,
            string sub,
            string issuer,
            string? email,
            string username,
            string scopes,
            DateTimeOffset issuedAt,
            DateTimeOffset expiresAt)
        {
            Id = id;
            Sub = sub;
            Issuer = issuer;
            Email = email;
            Username = username;
            Scopes = scopes;
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
        }
    }
}