using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

public sealed class DateTimeOffsetUnixTimeSecondsConverter : JsonConverter<DateTimeOffset?>
{
    public static DateTimeOffset? Deserialize(ref Utf8JsonReader reader)
        => reader.TokenType switch
            {
                JsonTokenType.Number => DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64()),
                JsonTokenType.Null => default(DateTimeOffset?),
                JsonTokenType jtokenType => throw new InvalidOperationException($"Expected {JsonTokenType.Number}, found {jtokenType}.")
            };

    public static void Serialize(Utf8JsonWriter writer, DateTimeOffset value)
        => writer.WriteNumberValue(value.ToUnixTimeSeconds());

    public static void Serialize(Utf8JsonWriter writer, DateTimeOffset? value)
    {
        if (value.HasValue)
        {
            Serialize(writer, value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Deserialize(ref reader);


    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        => Serialize(writer, value);
}