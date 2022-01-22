using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal
{
    public class ScopeCollectionConverter : JsonConverter<ScopeCollection>
    {
        public static ScopeCollectionConverter Instance { get; } = new ScopeCollectionConverter();

        private ScopeCollectionConverter() { }

        public override ScopeCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"Expected {JsonTokenType.StartArray}, got {reader.TokenType}.");
            }
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

        public override void Write(Utf8JsonWriter writer, ScopeCollection value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStartArray();
                foreach (var scope in value)
                {
                    writer.WriteStringValue(scope);
                }
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}