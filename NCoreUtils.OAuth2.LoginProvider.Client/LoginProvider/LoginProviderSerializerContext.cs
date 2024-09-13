using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.LoginProvider;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(JsonRootLoginProviderInfo))]
public partial class LoginProviderSerializerContext : JsonSerializerContext { }