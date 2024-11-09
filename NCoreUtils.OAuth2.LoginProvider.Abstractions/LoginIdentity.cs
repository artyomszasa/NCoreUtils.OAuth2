using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using NCoreUtils.Memory;

namespace NCoreUtils.OAuth2;

public partial class LoginIdentity
    : IEquatable<LoginIdentity>
// NOTE: compatibility / should be removed in uture releases
#pragma warning disable CS0618
    , IEmplaceable<ScopeCollection>
#pragma warning restore CS0618
    , ISpanExactEmplaceable
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

    [JsonPropertyName("sub")]
    public string Sub { get; }

    [JsonPropertyName("issuer")]
    public string Issuer { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("email")]
    public string? Email { get; }

    [JsonPropertyName("scopes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ScopeCollection Scopes { get; }

    [JsonPropertyName("custom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IReadOnlyDictionary<string, string?>? Custom { get; }

    [Obsolete("Use ctor with custom argument.")]
    public LoginIdentity(string sub, string issuer, string name, string? email, ScopeCollection scopes)
        : this(sub, issuer, name, email, scopes, default)
    { }

    [JsonConstructor]
    public LoginIdentity(string sub, string issuer, string name, string? email, ScopeCollection scopes, IReadOnlyDictionary<string, string?>? custom)
    {
        if (string.IsNullOrWhiteSpace(sub))
        {
            throw new ArgumentException("Sub must be a non-empty string.", nameof(sub));
        }
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new ArgumentException("Issuer must be a non-empty string.", nameof(issuer));
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be a non-empty string.", nameof(name));
        }
        Sub = sub;
        Issuer = issuer;
        Name = name;
        Email = email;
        Scopes = scopes;
        Custom = custom;
    }

    [Obsolete("Use GetEmplaceBufferSize instead.")]
    public int ComputeRequiredBufferSize() => GetEmplaceBufferSize();

    public int GetEmplaceBufferSize()
    {
        var total = Sub.Length + 1 + Name.Length;
        if (!string.IsNullOrEmpty(Email))
        {
            total += Email.Length + 2;
        }
        total += 3 + Issuer.Length + Scopes.ComputeRequiredBufferSize();
        if (Custom is IReadOnlyDictionary<string, string?> custom)
        {
            foreach (var (key, value) in custom)
            {
                total += 2 + key.Length;
                if (!string.IsNullOrEmpty(value))
                {
                    total += 3 + value.Length;
                }
            }
        }
        return total;
    }

    public bool Equals(LoginIdentity? other)
        => other is not null
            && Sub == other.Sub
            && Issuer == other.Issuer
            && Name == other.Name
            && Email == other.Email
            && Scopes == other.Scopes
            && CustomEq(Custom, other.Custom);

    public override bool Equals(object? obj)
        => obj is LoginIdentity other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Sub, Issuer, Name, Email, Scopes, Custom?.Count ?? 0);

    public override string ToString()
        => this.ToStringUsingArrayPool();

    public string ToString(string? format, IFormatProvider? provider)
        => ToString();

    public int Emplace(Span<char> span)
    {
        if (TryEmplace(span, out var size))
        {
            return size;
        }
        throw new InsufficientBufferSizeException(span, GetEmplaceBufferSize());
    }

    public bool TryEmplace(Span<char> span, out int used)
    {
        var builder = new SpanBuilder(span);
        if (!builder.TryAppend(Sub)
            || !builder.TryAppend('#')
            || !builder.TryAppend(Name))
        {
            goto failure;
        }
        if (Email is not null)
        {
            if (!builder.TryAppend('<')
                || !builder.TryAppend(Email)
                || !builder.TryAppend('>'))
            {
                goto failure;
            }
        }
        if (!builder.TryAppend('@')
            || !builder.TryAppend(Issuer)
            || !builder.TryAppend('[')
            || !builder.TryAppend(Scopes, ScopeCollection.Emplacer)
            || !builder.TryAppend(']'))
        {
            goto failure;
        }
        if (Custom is IReadOnlyDictionary<string, string?> custom)
        {
            foreach (var (key, value) in custom)
            {
                if (!builder.TryAppend(", ") || !builder.TryAppend(key)) { goto failure; }
                if (!string.IsNullOrEmpty(value))
                {
                    if (!builder.TryAppend(" = ") || !builder.TryAppend(value)) { goto failure; }
                }
            }
        }
        used = builder.Length;
        return true;
    failure:
        used = 0;
        return false;
    }
}