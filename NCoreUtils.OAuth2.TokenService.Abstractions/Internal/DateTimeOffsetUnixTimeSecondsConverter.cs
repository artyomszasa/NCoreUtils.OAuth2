using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

public sealed class DateTimeOffsetUnixTimeSecondsConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
            {
                JsonTokenType.Number => DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64()),
                JsonTokenType.Null => default(DateTimeOffset?),
                JsonTokenType jtokenType => throw new InvalidOperationException($"Expected {JsonTokenType.Number}, got {jtokenType}.")
            };

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value.ToUnixTimeSeconds());
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}