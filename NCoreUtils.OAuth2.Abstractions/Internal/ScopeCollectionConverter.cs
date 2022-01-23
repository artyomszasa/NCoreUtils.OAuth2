using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NCoreUtils.OAuth2.Internal;

public sealed class ScopeCollectionConverter : JsonConverter<ScopeCollection>
{
    private static Regex RegexWhitespace { get; } = new("\\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
            JsonTokenType.String => RegexWhitespace.Split(reader.GetString() ?? string.Empty),
            JsonTokenType.Null => default,
            // NOTE: for compatibility reasons and should be removed in future (!)
            JsonTokenType.StartArray => ReadAsArray(ref reader),
            JsonTokenType jtokenType => throw new JsonException($"Expected {JsonTokenType.String}, got {jtokenType}.")
        };

    public override void Write(Utf8JsonWriter writer, ScopeCollection value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(string.Join(" ", value._scopes));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}