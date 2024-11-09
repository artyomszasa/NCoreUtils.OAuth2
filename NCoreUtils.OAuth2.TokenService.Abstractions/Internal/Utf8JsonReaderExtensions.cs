using System.Runtime.CompilerServices;
using System.Text.Json;

namespace NCoreUtils.OAuth2.Internal;

internal static class Utf8JsonReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Expect(this in Utf8JsonReader reader, JsonTokenType expected)
    {
        var actual = reader.TokenType;
        if (actual != expected)
        {
            throw new JsonException($"Expected {expected}, found {actual}.");
        }
    }

    public static void ReadOrThrow(this ref Utf8JsonReader reader)
    {
        if (!reader.Read())
        {
            throw new JsonException("Unexpected end of JSON stream.");
        }
    }
}