using System;

namespace NCoreUtils.OAuth2;

public partial struct ScopeCollection
{
    bool ISpanEmplaceable.TryGetEmplaceBufferSize(out int minimumBufferSize)
    {
        minimumBufferSize = GetEmplaceBufferSize();
        return true;
    }

    bool ISpanEmplaceable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => TryEmplace(destination, out charsWritten);
}