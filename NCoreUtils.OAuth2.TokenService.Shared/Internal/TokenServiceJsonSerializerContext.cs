using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSerializable(typeof(IntrospectionResponse))]
[JsonSerializable(typeof(ErrorResponse))]
public partial class TokenServiceJsonSerializerContext : JsonSerializerContext
{ }