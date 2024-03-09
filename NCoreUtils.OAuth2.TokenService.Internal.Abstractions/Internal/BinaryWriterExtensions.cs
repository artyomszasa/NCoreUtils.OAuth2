using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NCoreUtils.OAuth2.Internal;

public static class BinaryWriterExtensions
{
    private static UTF8Encoding Utf8 { get; } = new(false);

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
        Span<byte> buffer = stackalloc byte[Utf8.GetMaxByteCount(value.Length)];
        var size = Utf8.GetBytes(value.AsSpan(), buffer);
        writer.Write(size);
        writer.Write(buffer[..size]);
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