using System;
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
    private const int MaxCharBufferStackAllocSize = 8 * 1024;

    private const int MaxCharBufferPoolAllocSize = 32 * 1024;

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

    [JsonConstructor]
    public LoginIdentity(string sub, string issuer, string name, string? email, ScopeCollection scopes)
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
    }

    private int EmplaceNoCheck(Span<char> buffer)
    {
        var builder = new SpanBuilder(buffer);
        builder.Append(Sub);
        builder.Append('#');
        builder.Append(Name);
        if (null != Email)
        {
            builder.Append('<');
            builder.Append(Email);
            builder.Append('>');
        }
        builder.Append('@');
        builder.Append(Issuer);
        builder.Append('[');
        builder.Append(Scopes);
        builder.Append(']');
        return builder.Length;
    }

    private string ToStringFallback(int requiredSize)
    {
        var builder = new StringBuilder(requiredSize);
        builder.Append(Sub);
        builder.Append('#');
        builder.Append(Name);
        if (null != Email)
        {
            builder.Append('<');
            builder.Append(Email);
            builder.Append('>');
        }
        builder.Append('@');
        builder.Append(Issuer);
        builder.Append('[');
        builder.Append(Scopes);
        builder.Append(']');
        return builder.ToString();
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
        return total;
    }

    public bool Equals(LoginIdentity? other)
        => other is not null
            && Sub == other.Sub
            && Issuer == other.Issuer
            && Name == other.Name
            && Email == other.Email
            && Scopes == other.Scopes;

    public override bool Equals(object? obj)
        => obj is LoginIdentity other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Sub, Issuer, Name, Email, Scopes);

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
            used = 0;
            return false;
        }
        if (null != Email)
        {
            if (!builder.TryAppend('<')
                || !builder.TryAppend(Email)
                || !builder.TryAppend('>'))
            {
                used = 0;
                return false;
            }
        }
        if (!builder.TryAppend('@')
            || !builder.TryAppend(Issuer)
            || !builder.TryAppend('[')
            || !builder.TryAppend(Scopes)
            || !builder.TryAppend(']'))
        {
            used = 0;
            return false;
        }
        used = builder.Length;
        return true;
    }
}