using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

public sealed class TimeSpanSecondsConverter : JsonConverter<TimeSpan>
{
    public static TimeSpan Deserialize(ref Utf8JsonReader reader)
        => reader.TokenType switch
        {
            JsonTokenType.Number => reader.TryGetInt64(out var seconds)
                ? TimeSpan.FromSeconds(seconds)
                : TimeSpan.FromSeconds(reader.GetDouble()),
            JsonTokenType.String => TimeSpan.Parse(reader.GetString() ?? string.Empty, CultureInfo.InvariantCulture),
            var tokenType => throw new JsonException($"Expected {JsonTokenType.Number} found {tokenType} while deserializing seconds.")
        };

    public static void Serialize(Utf8JsonWriter writer, TimeSpan value)
        => writer.WriteNumberValue((int)Math.Round(value.TotalSeconds));

    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Deserialize(ref reader);

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => Serialize(writer, value);
}