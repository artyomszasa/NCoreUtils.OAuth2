using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NCoreUtils.OAuth2.Internal
{
    public static class ScopeCollectionConverterExtensions
    {
        private static Regex _regexWhitespace = new Regex("\\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[]? ReadScopeCollection(this in Utf8JsonReader reader)
            => reader.TokenType switch
                {
                    JsonTokenType.String => _regexWhitespace.Split(reader.GetString()),
                    JsonTokenType.Null => default,
                    JsonTokenType jtokenType => throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {jtokenType}.")
                };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteScopeCollection(this Utf8JsonWriter writer, in JsonEncodedText propertyName, in ScopeCollection scopeCollection)
            => writer.WriteString(propertyName, string.Join(" ", scopeCollection));
    }
}