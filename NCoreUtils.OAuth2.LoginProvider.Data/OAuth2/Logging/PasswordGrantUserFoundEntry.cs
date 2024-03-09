using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Memory;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2.Logging;

public sealed class PaswordGrantUserFoundEntry<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TId>(bool useEmailAsUsername, string username, IUser<TId> user)
    where TId : IConvertible
{
    private sealed class GuidEmplacer : IEmplacer<Guid>
    {
        public int Emplace(Guid value, Span<char> span)
            => value.TryFormat(span, out var used)
                ? used
                : throw new InvalidOperationException("Insufficient buffer size.");

        public bool TryEmplace(Guid value, Span<char> span, out int used)
            => value.TryFormat(span, out used);
    }

    private static readonly Dictionary<Type, object> _emplacers = new(new Dictionary<Type, object>
    {
        { typeof(sbyte), Int8Emplacer.Instance },
        { typeof(short), Int16Emplacer.Instance },
        { typeof(int), Int32Emplacer.Instance },
        { typeof(long), Int64Emplacer.Instance },
        { typeof(byte), UInt8Emplacer.Instance },
        { typeof(ushort), UInt16Emplacer.Instance },
        { typeof(uint), UInt32Emplacer.Instance },
        { typeof(ulong), UInt64Emplacer.Instance },
        { typeof(char), CharEmplacer.Instance },
        { typeof(string), StringEmplacer.Instance },
        { typeof(Guid), new GuidEmplacer() }
    });

    private string? _cachedString;

    public bool UseEmailAsUsername { get; } = useEmailAsUsername;

    public string Username { get; } = username ?? string.Empty;

    public IUser<TId> User { get; } = user ?? throw new ArgumentNullException(nameof(user));

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Expected types are handled, unusual types can be preserved when used.")]
    [UnconditionalSuppressMessage("Trimming", "IL3050", Justification = "Expected types are handled, unusual types can be preserved when used.")]
    private static bool TryAppendSub(ref SpanBuilder builder, TId id)
    {
        if (_emplacers.TryGetValue(typeof(TId), out var emplacer))
        {
            return builder.TryAppend(id, (IEmplacer<TId>)emplacer);
        }
        // NOTE: unsafe fallback
        return builder.TryAppend(id);
    }

    private bool TryToStringNoAlloc([NotNullWhen(true)] out string? result)
    {
        Span<char> buffer = stackalloc char[4 * 1024];
        var builder = new SpanBuilder(buffer);
        result = default;
        if (!builder.TryAppend("Found user ")) { return false; }
        if (!TryAppendSub(ref builder, User.Sub)) { return false; }
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