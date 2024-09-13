using System.Text.Json.Serialization;
using NCoreUtils.OAuth2.LoginProvider;

namespace NCoreUtils.AspNetCore.Internal;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(JsonRootLoginProviderInfo))]
public partial class LoginProviderSerializerContext : JsonSerializerContext { }