using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal
{
    public sealed class LoginIdentityConverter : JsonConverter<LoginIdentity>
    {
        private static byte[] _binSub = Encoding.ASCII.GetBytes("sub");

        private static byte[] _binIss = Encoding.ASCII.GetBytes("iss");

        private static byte[] _binName = Encoding.ASCII.GetBytes("name");

        private static byte[] _binEmail = Encoding.ASCII.GetBytes("email");

        private static byte[] _binScopes = Encoding.ASCII.GetBytes("scopes");

        private static JsonEncodedText _jsonSub = JsonEncodedText.Encode("sub");

        private static JsonEncodedText _jsonIss = JsonEncodedText.Encode("iss");

        private static JsonEncodedText _jsonName = JsonEncodedText.Encode("name");

        private static JsonEncodedText _jsonEmail = JsonEncodedText.Encode("email");

        private static JsonEncodedText _jsonScopes = JsonEncodedText.Encode("scopes");

        public static LoginIdentityConverter Instance { get; } = new LoginIdentityConverter();

        public override LoginIdentity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new InvalidOperationException($"Expected {JsonTokenType.StartObject}, got {reader.TokenType}.");
            }
            string? sub = default;
            string? issuer = default;
            string? name = default;
            string? email = default;
            List<string>? scopes = default;
            reader.Read();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new InvalidOperationException($"Expected {JsonTokenType.PropertyName}, got {reader.TokenType}.");
                }
                if (reader.ValueSpan.IsSame(_binSub))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {reader.TokenType}.");
                    }
                    sub = reader.GetString();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binIss))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {reader.TokenType}.");
                    }
                    issuer = reader.GetString();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binName))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {reader.TokenType}.");
                    }
                    name = reader.GetString();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binEmail))
                {
                    reader.Read();
                    email = reader.TokenType switch
                    {
                        JsonTokenType.String => reader.GetString(),
                        JsonTokenType.Null => default,
                        JsonTokenType jtokenType => throw new InvalidOperationException($"Expected {jtokenType}, got {reader.TokenType}.")
                    };
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binScopes))
                {
                    scopes = new List<string>(8);
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.StartArray)
                    {
                        throw new InvalidOperationException($"Expected {JsonTokenType.StartArray}, got {reader.TokenType}.");
                    }
                    for (reader.Read(); reader.TokenType != JsonTokenType.EndArray; reader.Read())
                    {
                        if (reader.TokenType != JsonTokenType.String)
                        {
                            throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {reader.TokenType}.");
                        }
                        scopes.Add(reader.GetString());
                    }
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
            return new LoginIdentity(
                sub: sub!,
                issuer: issuer!,
                name: name!,
                email: email,
                scopes: scopes
            );
        }

        public override void Write(Utf8JsonWriter writer, LoginIdentity value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            writer.WriteStartObject();
            writer.WriteString(_jsonSub, value.Sub);
            writer.WriteString(_jsonIss, value.Issuer);
            writer.WriteString(_jsonName, value.Name);
            if (null != value.Email)
            {
                writer.WriteString(_jsonEmail, value.Email);
            }
            if (value.Scopes.HasValue)
            {
                writer.WritePropertyName(_jsonScopes);
                writer.WriteStartArray();
                foreach (var scope in value.Scopes)
                {
                    writer.WriteStringValue(scope);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
    }
}