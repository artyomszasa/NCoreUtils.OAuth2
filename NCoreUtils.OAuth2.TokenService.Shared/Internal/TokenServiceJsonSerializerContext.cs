using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSerializable(typeof(IntrospectionResponse))]
[JsonSerializable(typeof(ErrorResponse))]
public partial class TokenServiceJsonSerializerContext : JsonSerializerContext
{ }