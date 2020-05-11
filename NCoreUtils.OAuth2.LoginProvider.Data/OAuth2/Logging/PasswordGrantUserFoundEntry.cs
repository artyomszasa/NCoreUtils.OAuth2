using System;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2.Logging
{
    public sealed class PaswordGrantUserFoundEntry<TId>
        where TId : IConvertible
    {
        private string? _cachedString;

        public bool UseEmailAsUsername { get; }

        public string Username { get; }

        public IUser<TId> User { get; }

        public PaswordGrantUserFoundEntry(bool useEmailAsUsername, string username, IUser<TId> user)
        {
            UseEmailAsUsername = useEmailAsUsername;
            Username = username ?? string.Empty;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        #if NETSTANDARD2_1
        private bool TryToStringNoAlloc([NotNullWhen(true)] out string? result)
        #else
        private bool TryToStringNoAlloc(out string result)
        #endif
        {
            Span<char> buffer = stackalloc char[4 * 1024];
            var builder = new SpanBuilder(buffer);
            #if NETSTANDARD2_1
            result = default;
            #else
            result = default!;
            #endif
            if (!builder.TryAppend("Found user ")) { return false; }
            if (!builder.TryAppend(User.Sub)) { return false; }
            if (!builder.TryAppend(" with available scopes [")) { return false; }
            var first = true;
            foreach (var scope in User.GetAvailableScopes())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (!builder.TryAppend(", ")) { return false; }
                }
                if (!builder.TryAppend(scope)) { return false; }
            }
            if (!builder.TryAppend("] for ")) { return false; }
            if (!builder.TryAppend(UseEmailAsUsername ? "email" : "username")) { return false; }
            if (!builder.TryAppend(" = ")) { return false; }
            if (!builder.TryAppend(Username)) { return false; }
            if (!builder.TryAppend('.')) { return false; }
            result = builder.ToString();
            return true;
        }

        public string ToStringInternal()
        {
            if (TryToStringNoAlloc(out var result))
            {
                return result;
            }
            return $"Found user {User.Sub} with available scopes [{string.Join(", ", User.GetAvailableScopes())}] for {(UseEmailAsUsername ? "email" : "username")} = {Username}.";
        }

        public override string ToString()
        {
            if (_cachedString is null)
            {
                _cachedString = ToStringInternal();
            }
            return _cachedString;
        }
    }
}