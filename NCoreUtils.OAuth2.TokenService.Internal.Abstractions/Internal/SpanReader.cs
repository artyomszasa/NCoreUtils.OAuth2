using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace NCoreUtils.OAuth2.Internal;

public ref struct SpanReader(ReadOnlySpan<byte> buffer)
{
    private static UTF8Encoding Utf8 { get; } = new(false);

    private readonly ReadOnlySpan<byte> _buffer = buffer;

    private int _position = 0;

    private readonly int Available
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.Length - _position;
    }

    private readonly ReadOnlySpan<byte> Remaining
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer[_position..];
    }

    public bool TryReadInt32(out int value)
    {
        if (Available < sizeof(int))
        {
            value = default;
            return false;
        }
        value = BitConverter.ToInt32(Remaining);
        _position += sizeof(int);
        return true;
    }

    public bool TryReadInt64(out long value)
    {
        if (Available < sizeof(long))
        {
            value = default;
            return false;
        }
        value = BitConverter.ToInt64(Remaining);
        _position += sizeof(long);
        return true;
    }

    public bool TryReadUtf8String(out string? value)
    {
        if (Available < sizeof(int))
        {
            value = default;
            return false;
        }
        var size = BitConverter.ToInt32(Remaining);
        if (Available < size + sizeof(int))
        {
            value = default;
            return false;
        }
        Span<char> buffer = stackalloc char[size];
        var ssize = Utf8.GetChars(Remaining.Slice(sizeof(int), size), buffer);
        value = buffer[..ssize].ToString();
        _position += sizeof(int) + size;
        return true;
    }

    public bool TryReadUtf8Strings([NotNullWhen(true)] out IReadOnlyList<string>? value)
    {
        if (!TryReadInt32(out var count))
        {
            value = default;
            return false;
        }
        var pos = _position - sizeof(int);
        var list = new List<string>(count);
        for (var i = 0; i < count; ++i)
        {
            if (!TryReadUtf8String(out var svalue))
            {
                _position = pos;
                value = default;
                return false;
            }
            list.Add(svalue!);
        }
        value = list;
        return true;
    }
}