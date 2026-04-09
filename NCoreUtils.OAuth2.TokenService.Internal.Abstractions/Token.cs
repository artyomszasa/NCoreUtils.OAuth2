using System.Diagnostics.CodeAnalysis;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2;

public class Token : IEquatable<Token>
{
    private static bool CustomEq(IReadOnlyDictionary<string, string?>? a, IReadOnlyDictionary<string, string?>? b)
    {
        if (a is { Count: >0 })
        {
            if (b is null || b.Count != a.Count)
            {
                return false;
            }
            // NOTE: if a.Count == b.Count then it is enough to check that b contains all keys in a with the same value.
            foreach (var (akey, avalue) in a)
            {
                if (!b.TryGetValue(akey, out var bvalue) || !StringComparer.InvariantCulture.Equals(avalue, bvalue))
                {
                    return false;
                }
            }
            return true;
        }
        return b is null || b.Count == 0;
    }

    public static bool TryReadFrom(ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out Token? token)
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
            Dictionary<string, string?>? custom = default;
            if (reader.Available != 0)
            {
                if (!reader.TryReadInt32(out var count)) { goto failure; }
                if (count > 0)
                {
                    custom = new(StringComparer.InvariantCulture);
                    for (var i = 0; i < count; ++i)
                    {
                        if (!reader.TryReadUtf8String(out var key) || string.IsNullOrEmpty(key)) { goto failure; }
                        if (!reader.TryReadUtf8String(out var value)) { goto failure; }
                        custom.Add(key, value);
                    }
                }
            }
            token = new Token(
                tokenType!,
                sub!,
                issuer!,
                email,
                username!,
                scopes,
                new DateTimeOffset(issuedAtTicks, TimeSpan.Zero),
                new DateTimeOffset(expiresAtTicks, TimeSpan.Zero),
                custom
            );
            return true;
        }
    failure:
        token = default;
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

    public IReadOnlyDictionary<string, string?>? Custom { get; }

    [Obsolete("Use ctor with custom argument.")]
    public Token(
        string tokenType,
        string sub,
        string issuer,
        string? email,
        string username,
        IReadOnlyList<string> scopes,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt)
        : this(tokenType, sub, issuer, email, username, scopes, issuedAt, expiresAt, default)
    { }

    public Token(
        string tokenType,
        string sub,
        string issuer,
        string? email,
        string username,
        IReadOnlyList<string> scopes,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt,
        IReadOnlyDictionary<string, string?>? custom)
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
        Custom = custom;
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
            if (Custom is { Count: >0 } custom)
            {
                if (!writer.TryWriteInt32(custom.Count)) { goto failure; }
                foreach (var (key, value) in custom)
                {
                    if (!writer.TryWriteUtf8String(key)) { goto failure; }
                    if (!writer.TryWriteUtf8String(value)) { goto failure; }
                }
            }
            else
            {
                if (!writer.TryWriteInt32(0)) { goto failure; }
            }
            size = writer.Written;
            return true;
        }
    failure:
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
        if (Custom is { Count: >0 } custom)
        {
            writer.Write(custom.Count);
            foreach (var (key, value) in custom)
            {
                writer.WriteUtf8String(key);
                writer.WriteUtf8String(value);
            }
        }
    }

    public bool Equals(Token? other)
        => other is not null
            && TokenType == other.TokenType
            && Sub == other.Sub
            && Issuer == other.Issuer
            && Email == other.Email
            && Username == other.Username
            && Scopes.SequenceEqual(other.Scopes)
            && IssuedAt == other.IssuedAt
            && ExpiresAt == other.ExpiresAt
            && CustomEq(Custom, other.Custom);

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
        hash.Add(Custom is null ? 0 : Custom.Count);
        return hash.ToHashCode();
    }
}