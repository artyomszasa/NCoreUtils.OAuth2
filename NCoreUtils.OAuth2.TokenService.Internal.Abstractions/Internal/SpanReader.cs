using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
#if !NETSTANDARD2_1
using BitConverter = NCoreUtils.PolyfillBitConverter;
#endif

namespace NCoreUtils.OAuth2.Internal
{
    public ref struct SpanReader
    {
        private static readonly UTF8Encoding _utf8 = new UTF8Encoding(false);

        private readonly ReadOnlySpan<byte> _buffer;

        private int _position;

        private int Available
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length - _position;
        }

        private ReadOnlySpan<byte> Remaining
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Slice(_position);
        }

        public SpanReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
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
            var ssize = _utf8.GetChars(Remaining.Slice(sizeof(int), size), buffer);
            value = buffer.Slice(0, ssize).ToString();
            _position += sizeof(int) + size;
            return true;
        }

        #if NETSTANDARD2_1
        public bool TryReadUtf8Strings([NotNullWhen(true)] out IReadOnlyList<string>? value)
        #else
        public bool TryReadUtf8Strings(out IReadOnlyList<string> value)
        #endif
        {
            if (!TryReadInt32(out var count))
            {
                #if NETSTANDARD2_1
                value = default;
                #else
                value = default!;
                #endif
                return false;
            }
            var pos = _position - sizeof(int);
            var list = new List<string>(count);
            for (var i = 0; i < count; ++i)
            {
                if (!TryReadUtf8String(out var svalue))
                {
                    _position = pos;
                    #if NETSTANDARD2_1
                    value = default;
                    #else
                    value = default!;
                    #endif
                    return false;
                }
                list.Add(svalue!);
            }
            value = list;
            return true;
        }
    }
}