using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    public class Token : IEquatable<Token>
    {
        #if NETSTANDARD2_1
        public static bool TryReadFrom(ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out Token? token)
        #else
        public static bool TryReadFrom(ReadOnlySpan<byte> buffer, out Token token)
        #endif
        {
            var reader = new SpanReader(buffer);
            if (reader.TryReadUtf8String(out var tokenType)
                && reader.TryReadUtf8String(out var sub)
                && reader.TryReadUtf8String(out var issuer)
                && reader.TryReadUtf8String(out var email)
                && reader.TryReadUtf8String(out var username)
                && reader.TryReadUtf8Strings(out var scopes)
                && reader.TryReadInt64(out var issuedAtTicks)
                && reader.TryReadInt64(out var expiresAtTicks))
            {
                token = new Token(
                    tokenType!,
                    sub!,
                    issuer!,
                    email,
                    username!,
                    scopes,
                    new DateTimeOffset(issuedAtTicks, TimeSpan.Zero),
                    new DateTimeOffset(expiresAtTicks, TimeSpan.Zero));
                return true;
            }
            #if NETSTANDARD2_1
            token = default;
            #else
            token = default!;
            #endif
            return false;
        }

        public string TokenType { get; }

        public string Sub { get; }

        public string Issuer { get; }

        public string? Email { get; }

        public string Username { get; }

        public IReadOnlyList<string> Scopes { get; }

        public DateTimeOffset IssuedAt { get; }

        public DateTimeOffset ExpiresAt { get; }

        public Token(
            string tokenType,
            string sub,
            string issuer,
            string? email,
            string username,
            IReadOnlyList<string> scopes,
            DateTimeOffset issuedAt,
            DateTimeOffset expiresAt)
        {
            if (string.IsNullOrWhiteSpace(tokenType))
            {
                throw new ArgumentException("Token type must be a non-empty string.", nameof(tokenType));
            }
            if (string.IsNullOrWhiteSpace(sub))
            {
                throw new ArgumentException("Subject must be a non-empty string.", nameof(sub));
            }
            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentException("Issuer must be a non-empty string.", nameof(sub));
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username must be a non-empty string.", nameof(username));
            }
            TokenType = tokenType;
            Sub = sub;
            Issuer = issuer;
            Email = email;
            Username = username;
            Scopes = scopes;
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
        }

        public bool TryWriteTo(Span<byte> buffer, out int size)
        {
            var writer = new SpanWriter(buffer);
            if (writer.TryWriteUtf8String(TokenType)
                && writer.TryWriteUtf8String(Sub)
                && writer.TryWriteUtf8String(Issuer)
                && writer.TryWriteUtf8String(Email)
                && writer.TryWriteUtf8String(Username)
                && writer.TryWriteUtf8Strings(Scopes)
                && writer.TryWriteInt64(IssuedAt.UtcTicks)
                && writer.TryWriteInt64(ExpiresAt.UtcTicks))
            {
                size = writer.Written;
                return true;
            }
            size = default;
            return false;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.WriteUtf8String(TokenType);
            writer.WriteUtf8String(Sub);
            writer.WriteUtf8String(Issuer);
            writer.WriteUtf8String(Email);
            writer.WriteUtf8String(Username);
            writer.WriteUtf8Strings(Scopes);
            writer.Write(IssuedAt.UtcTicks);
            writer.Write(ExpiresAt.UtcTicks);
        }

        public bool Equals(Token other)
            => null != other
                && TokenType == other.TokenType
                && Sub == other.Sub
                && Issuer == other.Issuer
                && Email == other.Email
                && Username == other.Username
                && Scopes.SequenceEqual(other.Scopes)
                && IssuedAt == other.IssuedAt
                && ExpiresAt == other.ExpiresAt;

        public override bool Equals(object? obj)
            => obj is Token other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(TokenType);
            hash.Add(Sub);
            hash.Add(Issuer);
            hash.Add(Email);
            hash.Add(Username);
            hash.Add(Scopes.Count);
            foreach (var scope in Scopes)
            {
                hash.Add(scope);
            }
            hash.Add(IssuedAt);
            hash.Add(ExpiresAt);
            return hash.ToHashCode();
        }
    }
}