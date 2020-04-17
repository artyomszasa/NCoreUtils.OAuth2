using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal
{
    public class ErrorResponseConverter : JsonConverter<ErrorResponse>
    {
        private static byte[] _binError = Encoding.ASCII.GetBytes("error");

        private static byte[] _binErrorDescription = Encoding.ASCII.GetBytes("error_description");

        private static JsonEncodedText _jsonError = JsonEncodedText.Encode("error");

        private static JsonEncodedText _jsonErrorDescription = JsonEncodedText.Encode("error_description");

        public static ErrorResponseConverter Instance { get; } = new ErrorResponseConverter();

        private ErrorResponseConverter() { }

        public override ErrorResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new InvalidOperationException($"Expected {JsonTokenType.StartObject}, got {reader.TokenType}.");
            }
            string? error = default;
            string? errorDescription = default;
            reader.Read();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new InvalidOperationException($"Expected {JsonTokenType.PropertyName}, got {reader.TokenType}.");
                }
                if (reader.ValueSpan.IsSame(_binError))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {reader.TokenType}.");
                    }
                    error = reader.GetString();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binErrorDescription))
                {
                    reader.Read();
                    errorDescription = reader.ReadStringOrDefault();
                    reader.Read();
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                    reader.Read();
                }
            }
            reader.Read();
            return new ErrorResponse(error!, errorDescription);
        }

        public override void Write(Utf8JsonWriter writer, ErrorResponse value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            writer.WriteStartObject();
            writer.WriteString(_jsonError, value.ErrorCode);
            if (null != value.ErrorDescription)
            {
                writer.WriteString(_jsonErrorDescription, value.ErrorDescription);
            }
            writer.WriteEndObject();
        }
    }
}