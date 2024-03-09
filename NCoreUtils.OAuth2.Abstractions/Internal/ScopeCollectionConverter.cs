using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NCoreUtils.OAuth2.Internal;

public sealed class ScopeCollectionConverter : JsonConverter<ScopeCollection>
{

#if NET6_0_OR_GREATER
    private static char[] WsChars { get; } = [ ' ', '\t', '\r', '\n' ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string[] SplitScopes(string input)
        => input.Split(WsChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
#else
    private static Regex RegexWhitespace { get; } = new("\\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string[] SplitScopes(string input)
        => RegexWhitespace.Split(input);
#endif

    private static ScopeCollection ReadAsArray(ref Utf8JsonReader reader)
    {
        reader.Read();
        var scopes = new List<string>(8);
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    break;
                case JsonTokenType.String:
                    scopes.Add(reader.GetString()!);
                    break;
                default:
                    throw new JsonException($"Expected {JsonTokenType.String}, got {reader.TokenType}.");
            }
            reader.Read();
        }
        return scopes;
    }

    public override ScopeCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => SplitScopes(reader.GetString() ?? string.Empty),
            JsonTokenType.Null => default,
            // NOTE: for compatibility reasons and should be removed in future (!)
            JsonTokenType.StartArray => ReadAsArray(ref reader),
            JsonTokenType jtokenType => throw new JsonException($"Expected {JsonTokenType.String}, got {jtokenType}.")
        };

    public override void Write(Utf8JsonWriter writer, ScopeCollection value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(string.Join(' ', value._scopes));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}