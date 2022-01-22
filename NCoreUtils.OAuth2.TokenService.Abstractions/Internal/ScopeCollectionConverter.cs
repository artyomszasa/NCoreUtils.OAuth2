using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NCoreUtils.OAuth2.Internal;

public sealed class ScopeCollectionConverter : JsonConverter<ScopeCollection>
{
    private static Regex RegexWhitespace { get; } = new("\\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public override ScopeCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => RegexWhitespace.Split(reader.GetString() ?? string.Empty),
            JsonTokenType.Null => default,
            JsonTokenType jtokenType => throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {jtokenType}.")
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