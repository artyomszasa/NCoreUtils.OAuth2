namespace NCoreUtils.OAuth2;

#if NETFRAMEWORK

internal static class LoginIdentityCompactExtensions
{
    public static void Deconstruct(this KeyValuePair<string, string?> source, out string key, out string? value)
    {
        key = source.Key;
        value = source.Value;
    }
}

#endif

public partial class LoginIdentity
{
    bool ISpanEmplaceable.TryGetEmplaceBufferSize(out int minimumBufferSize)
    {
        minimumBufferSize = GetEmplaceBufferSize();
        return true;
    }

    bool ISpanEmplaceable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => TryEmplace(destination, out charsWritten);
}