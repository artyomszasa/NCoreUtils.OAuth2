using System.Text.Json.Serialization;
using NCoreUtils.OAuth2.LoginProvider;

namespace NCoreUtils.AspNetCore.Internal;

[JsonSerializable(typeof(JsonRootILoginProvider))]
public partial class LoginProviderSerializerContext : JsonSerializerContext { }