using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

[JsonSerializable(typeof(JsonRootTokenServiceEndpointsInfo))]
public partial class TokenServiceSerializerContext : JsonSerializerContext { }