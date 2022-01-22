using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LoginIdentity))]
[JsonSerializable(typeof(ScopeCollection))]
[JsonSerializable(typeof(string))]
public partial class LoginProviderSerializationContext : JsonSerializerContext
{ }