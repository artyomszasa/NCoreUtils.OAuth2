using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.LoginProvider;

[JsonSerializable(typeof(JsonRootILoginProvider))]
public partial class LoginProviderSerializerContext : JsonSerializerContext { }