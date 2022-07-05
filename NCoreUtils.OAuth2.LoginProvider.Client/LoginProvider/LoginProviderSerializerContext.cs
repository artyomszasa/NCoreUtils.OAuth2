using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.LoginProvider;

[JsonSerializable(typeof(JsonRootLoginProviderInfo))]
public partial class LoginProviderSerializerContext : JsonSerializerContext { }