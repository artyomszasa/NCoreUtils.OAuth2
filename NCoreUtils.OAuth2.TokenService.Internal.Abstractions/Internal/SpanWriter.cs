using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;


namespace NCoreUtils.OAuth2.Internal;

public ref struct SpanWriter(Span<byte> buffer)
{
    private static UTF8Encoding Utf8 { get; } = new(false);

    private readonly Span<byte> _buffer = buffer;

    private int _position = 0;

    private readonly int Available
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.Length - _position;
    }

    private readonly Span<byte> Remaining
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer[_position..];
    }

    public readonly int Written
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWriteInt32(int value)
    {
        if (BitConverter.TryWriteBytes(Remaining, value))
        {
            _position += sizeof(int);
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWriteInt64(long value)
    {
        if (BitConverter.TryWriteBytes(Remaining, value))
        {
            _position += sizeof(long);
            return true;
        }
        return false;
    }

    public bool TryWriteUtf8String(string? value)
    {
        if (value is null)
        {
            return TryWriteInt32(-1);
        }
        if (0 == value.Length)
        {
            return TryWriteInt32(0);
        }
        Span<byte> buffer = stackalloc byte[Utf8.GetMaxByteCount(value.Length)];
        var size = Utf8.GetBytes(value.AsSpan(), buffer);
        if (Available < size + sizeof(int))
        {
            return false;
        }
        TryWriteInt32(size);
        buffer[..size].CopyTo(Remaining);
        _position += size;
        return true;
    }

    public bool TryWriteUtf8Strings(IReadOnlyList<string> value)
    {
        var pos = _position;
        if (!TryWriteInt32(value.Count))
        {
            return false;
        }
        foreach (var item in value)
        {
            if (!TryWriteUtf8String(item))
            {
                _position = pos;
                return false;
            }
        }
        return true;
    }
}