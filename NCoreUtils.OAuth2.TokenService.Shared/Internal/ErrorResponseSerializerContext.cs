using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ErrorResponse))]
public partial class ErrorResponseSerializerContext : JsonSerializerContext { }