using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(JsonRootTokenServiceEndpointsInfo))]
public partial class TokenServiceSerializerContext : JsonSerializerContext { }