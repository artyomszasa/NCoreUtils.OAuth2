using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal
{
    public class IntrospectionResponseConverter : JsonConverter<IntrospectionResponse>
    {
        private static byte[] _binActive = Encoding.ASCII.GetBytes("active");

        private static byte[] _binScope = Encoding.ASCII.GetBytes("scope");

        private static byte[] _binClientId = Encoding.ASCII.GetBytes("client_id");

        private static byte[] _binEmail = Encoding.ASCII.GetBytes("email");

        private static byte[] _binUsername = Encoding.ASCII.GetBytes("username");

        private static byte[] _binTokenType = Encoding.ASCII.GetBytes("token_type");

        private static byte[] _binExp = Encoding.ASCII.GetBytes("exp");

        private static byte[] _binIat = Encoding.ASCII.GetBytes("iat");

        private static byte[] _binNbf = Encoding.ASCII.GetBytes("nbf");

        private static byte[] _binSub = Encoding.ASCII.GetBytes("sub");

        private static byte[] _binIss = Encoding.ASCII.GetBytes("iss");

        private static JsonEncodedText _jsonActive = JsonEncodedText.Encode("active");

        private static JsonEncodedText _jsonScope = JsonEncodedText.Encode("scope");

        private static JsonEncodedText _jsonClientId = JsonEncodedText.Encode("client_id");

        private static JsonEncodedText _jsonEmail = JsonEncodedText.Encode("email");

        private static JsonEncodedText _jsonUsername = JsonEncodedText.Encode("username");

        private static JsonEncodedText _jsonTokenType = JsonEncodedText.Encode("token_type");

        private static JsonEncodedText _jsonExp = JsonEncodedText.Encode("exp");

        private static JsonEncodedText _jsonIat = JsonEncodedText.Encode("iat");

        private static JsonEncodedText _jsonNbf = JsonEncodedText.Encode("nbf");

        private static JsonEncodedText _jsonSub = JsonEncodedText.Encode("sub");

        private static JsonEncodedText _jsonIss = JsonEncodedText.Encode("iss");

        public static IntrospectionResponseConverter Instance { get; } = new IntrospectionResponseConverter();

        private IntrospectionResponseConverter() { }

        public override IntrospectionResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new InvalidOperationException($"Expected {JsonTokenType.StartObject}, got {reader.TokenType}.");
            }
            var active = false;
            string[]? scopes = default;
            string? clientId = default;
            string? email = default;
            string? username = default;
            string? tokenType = default;
            DateTimeOffset? expiresAt = default;
            DateTimeOffset? issuedAt = default;
            DateTimeOffset? notBefore = default;
            string? sub = default;
            string? issuer = default;
            reader.Read();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new InvalidOperationException($"Expected {JsonTokenType.PropertyName}, got {reader.TokenType}.");
                }
                if (reader.ValueSpan.IsSame(_binActive))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.False && reader.TokenType != JsonTokenType.True)
                    {
                        throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {reader.TokenType}.");
                    }
                    active = reader.GetBoolean();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binScope))
                {
                    reader.Read();
                    scopes = reader.ReadScopeCollection();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binClientId))
                {
                    reader.Read();
                    clientId = reader.ReadStringOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binEmail))
                {
                    reader.Read();
                    email = reader.ReadStringOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binUsername))
                {
                    reader.Read();
                    username = reader.ReadStringOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binTokenType))
                {
                    reader.Read();
                    tokenType = reader.ReadStringOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binEmail))
                {
                    reader.Read();
                    email = reader.ReadStringOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binExp))
                {
                    reader.Read();
                    expiresAt = reader.ReadDateTimeOffsetOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binIat))
                {
                    reader.Read();
                    issuedAt = reader.ReadDateTimeOffsetOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binNbf))
                {
                    reader.Read();
                    notBefore = reader.ReadDateTimeOffsetOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binSub))
                {
                    reader.Read();
                    sub = reader.ReadStringOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binIss))
                {
                    reader.Read();
                    issuer = reader.ReadStringOrDefault();
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
            return new IntrospectionResponse(
                active,
                scopes,
                clientId,
                email,
                username,
                tokenType,
                expiresAt,
                issuedAt,
                notBefore,
                sub,
                issuer
            );
        }

        public override void Write(Utf8JsonWriter writer, IntrospectionResponse value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            writer.WriteStartObject();
            writer.WriteBoolean(_jsonActive, value.Active);
            if (value.Scope.HasValue)
            {
                writer.WriteScopeCollection(_jsonScope, value.Scope);
            }
            if (null != value.ClientId)
            {
                writer.WriteString(_jsonClientId, value.ClientId);
            }
            if (null != value.Email)
            {
                writer.WriteString(_jsonEmail, value.Email);
            }
            if (null != value.Username)
            {
                writer.WriteString(_jsonUsername, value.Username);
            }
            if (null != value.TokenType)
            {
                writer.WriteString(_jsonTokenType, value.TokenType);
            }
            if (value.ExpiresAt.HasValue)
            {
                writer.WriteNumber(_jsonExp, value.ExpiresAt.Value.ToUnixTimeSeconds());
            }
            if (value.IssuedAt.HasValue)
            {
                writer.WriteNumber(_jsonIat, value.IssuedAt.Value.ToUnixTimeSeconds());
            }
            if (value.NotBefore.HasValue)
            {
                writer.WriteNumber(_jsonNbf, value.NotBefore.Value.ToUnixTimeSeconds());
            }
            if (null != value.Sub)
            {
                writer.WriteString(_jsonSub, value.Sub);
            }
            if (null != value.Issuer)
            {
                writer.WriteString(_jsonIss, value.Issuer);
            }
            writer.WriteEndObject();
        }
    }
}