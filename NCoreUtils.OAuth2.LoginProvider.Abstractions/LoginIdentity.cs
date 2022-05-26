using System;
using System.Buffers;
using System.Text;
using System.Text.Json.Serialization;
using NCoreUtils.Memory;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    public class LoginIdentity : IEquatable<LoginIdentity>, IEmplaceable<LoginIdentity>
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

        public int ComputeRequiredBufferSize()
        {
            var total = Sub.Length + 1 + Name.Length;
            if (!string.IsNullOrEmpty(Email))
            {
                total += Email.Length + 2;
            }
            total += 3 + Issuer.Length + Scopes.ComputeRequiredBufferSize();
            return total;
        }

        public int Emplace(Span<char> span)
        {
            if (TryEmplace(span, out var size))
            {
                return size;
            }
            var requiredSize = ComputeRequiredBufferSize();
            throw new InsufficientBufferSizeException(span, requiredSize);
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
        {
            var requiredSize = ComputeRequiredBufferSize();
            if (requiredSize <= MaxCharBufferStackAllocSize)
            {
                Span<char> buffer = stackalloc char[requiredSize];
                var size = EmplaceNoCheck(buffer);
                return new string(buffer[..size]);
            }
            if (requiredSize <= MaxCharBufferPoolAllocSize)
            {
                using var owner = MemoryPool<char>.Shared.Rent(requiredSize);
                var size = EmplaceNoCheck(owner.Memory.Span);
                return new string(owner.Memory.Span[..size]);
            }
            return ToStringFallback(requiredSize);
        }

        public bool TryEmplace(Span<char> span, out int used)
        {
            var requiredSize = ComputeRequiredBufferSize();
            if (requiredSize <= span.Length)
            {
                used = EmplaceNoCheck(span);
                return true;
            }
            used = default;
            return false;
        }
    }
}