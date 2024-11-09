using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

public sealed class IntrospectionResponseConverter : JsonConverter<IntrospectionResponse>
{
    public override IntrospectionResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Expect(JsonTokenType.StartObject);
        reader.ReadOrThrow();
        bool active = default;
        ScopeCollection scope = default;
        string? clientId = default;
        string? email = default;
        string? username = default;
        string? tokenType = default;
        DateTimeOffset? expiresAt = default;
        DateTimeOffset? issuedAt = default;
        DateTimeOffset? notBefore = default;
        string? sub = default;
        string? issuer = default;
        Dictionary<string, string?>? custom = default;
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.ValueTextEquals("active"u8))
            {
                reader.ReadOrThrow();
                active = reader.GetBoolean();
            }
            else if (reader.ValueTextEquals("scope"u8))
            {
                reader.ReadOrThrow();
                scope = ScopeCollectionConverter.Deserialize(ref reader);
            }
            else if (reader.ValueTextEquals("client_id"u8))
            {
                reader.ReadOrThrow();
                clientId = reader.GetString();
            }
            else if (reader.ValueTextEquals("email"u8))
            {
                reader.ReadOrThrow();
                email = reader.GetString();
            }
            else if (reader.ValueTextEquals("username"u8))
            {
                reader.ReadOrThrow();
                username = reader.GetString();
            }
            else if (reader.ValueTextEquals("token_type"u8))
            {
                reader.ReadOrThrow();
                tokenType = reader.GetString();
            }
            else if (reader.ValueTextEquals("exp"u8))
            {
                reader.ReadOrThrow();
                expiresAt = DateTimeOffsetUnixTimeSecondsConverter.Deserialize(ref reader);
            }
            else if (reader.ValueTextEquals("iat"u8))
            {
                reader.ReadOrThrow();
                issuedAt = DateTimeOffsetUnixTimeSecondsConverter.Deserialize(ref reader);
            }
            else if (reader.ValueTextEquals("nbf"u8))
            {
                reader.ReadOrThrow();
                notBefore = DateTimeOffsetUnixTimeSecondsConverter.Deserialize(ref reader);
            }
            else if (reader.ValueTextEquals("sub"u8))
            {
                reader.ReadOrThrow();
                sub = reader.GetString();
            }
            else if (reader.ValueTextEquals("iss"u8))
            {
                reader.ReadOrThrow();
                issuer = reader.GetString();
            }
            else
            {
                var key = reader.GetString() ?? string.Empty;
                reader.ReadOrThrow();
                (custom ??= [])[key] = reader.GetString();
            }
            reader.ReadOrThrow();
        }
        return new IntrospectionResponse(
            active,
            scope,
            clientId,
            email,
            username,
            tokenType,
            expiresAt,
            issuedAt,
            notBefore,
            sub,
            issuer,
            custom
        );
    }

    public override void Write(Utf8JsonWriter writer, IntrospectionResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("active"u8, value.Active);
        if (value.Active)
        {
            if (!value.Scope.IsEmpty)
            {
                writer.WritePropertyName("scope");
                ScopeCollectionConverter.Serialize(writer, value.Scope);
            }
            if (!string.IsNullOrEmpty(value.ClientId))
            {
                writer.WriteString("client_id"u8, value.ClientId);
            }
            if (!string.IsNullOrEmpty(value.Email))
            {
                writer.WriteString("email"u8, value.Email);
            }
            if (!string.IsNullOrEmpty(value.Username))
            {
                writer.WriteString("username"u8, value.Username);
            }
            if (!string.IsNullOrEmpty(value.TokenType))
            {
                writer.WriteString("token_type"u8, value.TokenType);
            }
            if (value.ExpiresAt is DateTimeOffset expiresAt)
            {
                writer.WritePropertyName("exp"u8);
                DateTimeOffsetUnixTimeSecondsConverter.Serialize(writer, expiresAt);
            }
            if (value.IssuedAt is DateTimeOffset issuedAt)
            {
                writer.WritePropertyName("iat"u8);
                DateTimeOffsetUnixTimeSecondsConverter.Serialize(writer, issuedAt);
            }
            if (value.NotBefore is DateTimeOffset notBefore)
            {
                writer.WritePropertyName("nbf"u8);
                DateTimeOffsetUnixTimeSecondsConverter.Serialize(writer, notBefore);
            }
            if (!string.IsNullOrEmpty(value.Sub))
            {
                writer.WriteString("sub"u8, value.Sub);
            }
            if (!string.IsNullOrEmpty(value.Issuer))
            {
                writer.WriteString("iss"u8, value.Issuer);
            }
            if (value.Custom is { Count: >0 } custom)
            {
                foreach (var (key, val) in custom)
                {
                    writer.WritePropertyName(key);
                    if (val is null)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        writer.WriteStringValue(val);
                    }
                }
            }
        }
        writer.WriteEndObject();
    }
}