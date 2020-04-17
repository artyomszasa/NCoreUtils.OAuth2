using System;

namespace NCoreUtils.OAuth2
{
    public class LoginIdentity : IEquatable<LoginIdentity>
    {
        public string Sub { get; }

        public string Issuer { get; }

        public string Name { get; }

        public string? Email { get; }

        public ScopeCollection Scopes { get; }

        public LoginIdentity(string sub, string issuer, string name, string? email, ScopeCollection scopes)
        {
            if (string.IsNullOrWhiteSpace(sub))
            {
                throw new ArgumentException("Sub must be a non-empty string.", nameof(sub));
            }
            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentException("Issuer must be a non-empty string.", nameof(sub));
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

        public bool Equals(LoginIdentity other)
            => other != null
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
            Span<char> buffer = stackalloc char[8 * 1024];
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
            return builder.ToString();
        }
    }
}