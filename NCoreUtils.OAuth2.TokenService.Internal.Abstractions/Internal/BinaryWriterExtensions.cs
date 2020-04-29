using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NCoreUtils.OAuth2.Internal
{
    public static class BinaryWriterExtensions
    {
        private static UTF8Encoding _utf8 = new UTF8Encoding(false);

        public static void WriteUtf8String(this BinaryWriter writer, string? value)
        {
            if (value is null)
            {
                writer.Write(-1);
                return;
            }
            if (value.Length == 0)
            {
                writer.Write(0);
                return;
            }
            #if NETSTANDARD2_1
            Span<byte> buffer = stackalloc byte[_utf8.GetMaxByteCount(value.Length)];
            var size = _utf8.GetBytes(value.AsSpan(), buffer);
            writer.Write(size);
            writer.Write(buffer.Slice(0, size));
            #else
            var bytes = _utf8.GetBytes(value);
            writer.Write(bytes.Length);
            writer.Write(bytes, 0, bytes.Length);
            #endif
        }

        public static void WriteUtf8Strings(this BinaryWriter writer, IReadOnlyList<string> value)
        {
            writer.Write(value.Count);
            foreach (var s in value)
            {
                writer.WriteUtf8String(s);
            }
        }
    }
}