using System.Text.Json.Serialization;

namespace NCoreUtils.OAuth2.Internal;

[JsonSerializable(typeof(ErrorResponse))]
public partial class ErrorResponseSerializerContext : JsonSerializerContext { }