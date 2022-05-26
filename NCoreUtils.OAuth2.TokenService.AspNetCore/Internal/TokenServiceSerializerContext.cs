using System.Text.Json.Serialization;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.AspNetCore.Internal;

[JsonSerializable(typeof(JsonRootITokenServiceEndpoints))]
public partial class TokenServiceSerializerContext : JsonSerializerContext { }