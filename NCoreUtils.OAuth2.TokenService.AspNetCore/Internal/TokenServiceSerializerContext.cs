using System.Text.Json.Serialization;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.AspNetCore.Internal;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(JsonRootTokenServiceEndpointsInfo))]
public partial class TokenServiceSerializerContext : JsonSerializerContext { }