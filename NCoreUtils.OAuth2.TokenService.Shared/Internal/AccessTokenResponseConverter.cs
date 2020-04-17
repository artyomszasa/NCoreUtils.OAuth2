using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NCoreUtils.OAuth2.Internal
{
    public class AccessTokenResponseConverter : JsonConverter<AccessTokenResponse>
    {
        private static byte[] _binAccessToken = Encoding.ASCII.GetBytes("access_token");

        private static byte[] _binTokenType = Encoding.ASCII.GetBytes("token_type");

        private static byte[] _binExpiresIn = Encoding.ASCII.GetBytes("expires_in");

        private static byte[] _binRefreshToken = Encoding.ASCII.GetBytes("refresh_token");

        private static byte[] _binScope = Encoding.ASCII.GetBytes("scope");

        private static JsonEncodedText _jsonAccessToken = JsonEncodedText.Encode("access_token");

        private static JsonEncodedText _jsonTokenType = JsonEncodedText.Encode("token_type");

        private static JsonEncodedText _jsonExpiresIn = JsonEncodedText.Encode("expires_in");

        private static JsonEncodedText _jsonRefreshToken = JsonEncodedText.Encode("refresh_token");

        private static JsonEncodedText _jsonScope = JsonEncodedText.Encode("scope");

        public static AccessTokenResponseConverter Instance { get; } = new AccessTokenResponseConverter();

        private AccessTokenResponseConverter() { }

        public override AccessTokenResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new InvalidOperationException($"Expected {JsonTokenType.StartObject}, got {reader.TokenType}.");
            }
            string? accessToken = default;
            string? tokenType = default;
            TimeSpan? expiresIn = default;
            string? refreshToken = default;
            string[]? scopes = default;
            reader.Read();
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new InvalidOperationException($"Expected {JsonTokenType.PropertyName}, got {reader.TokenType}.");
                }
                if (reader.ValueSpan.IsSame(_binAccessToken))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new InvalidOperationException($"Expected {JsonTokenType.String}, got {reader.TokenType}.");
                    }
                    accessToken = reader.GetString();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binTokenType))
                {
                    reader.Read();
                    tokenType = reader.ReadStringOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binExpiresIn))
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.Number)
                    {
                        throw new InvalidOperationException($"Expected {JsonTokenType.Number}, got {reader.TokenType}.");
                    }
                    expiresIn = TimeSpan.FromSeconds(reader.GetInt32());
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binRefreshToken))
                {
                    reader.Read();
                    refreshToken = reader.ReadStringOrDefault();
                    reader.Read();
                }
                else if (reader.ValueSpan.IsSame(_binScope))
                {
                    reader.Read();
                    scopes = reader.ReadScopeCollection();
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
            return new AccessTokenResponse(
                accessToken ?? string.Empty,
                tokenType,
                expiresIn ?? default,
                refreshToken,
                scopes
            );
        }

        public override void Write(Utf8JsonWriter writer, AccessTokenResponse value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            writer.WriteStartObject();
            writer.WriteString(_jsonAccessToken, value.AccessToken);
            if (null != value.TokenType)
            {
                writer.WriteString(_jsonTokenType, value.TokenType);
            }
            writer.WriteNumber(_jsonExpiresIn, (int)Math.Round(value.ExpiresIn.TotalSeconds));
            if (null != value.RefreshToken)
            {
                writer.WriteString(_jsonRefreshToken, value.RefreshToken);
            }
            if (value.Scope.HasValue)
            {
                writer.WriteScopeCollection(_jsonScope, value.Scope);
            }
            writer.WriteEndObject();
        }
    }
}