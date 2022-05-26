using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

[JsonSerializable(typeof(JsonRootITokenServiceEndpoints))]
public partial class TokenServiceSerializerContext : JsonSerializerContext { }