using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace NCoreUtils.OAuth2.Internal
{
    internal static class Utf8ReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? ReadStringOrDefault(this in Utf8JsonReader reader)
            => reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Null => default,
                JsonTokenType jtokenType => throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {jtokenType}.")
            };

        internal static DateTimeOffset? ReadDateTimeOffsetOrDefault(this in Utf8JsonReader reader)
            => reader.TokenType switch
            {
                JsonTokenType.Number => (DateTimeOffset?)DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64()),
                JsonTokenType.Null => default,
                JsonTokenType jtokenType => throw new InvalidOperationException($"Expected {JsonTokenType.Number}, got {jtokenType}.")
            };
    }
}