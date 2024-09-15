using System.Collections.Generic;
using System.Text.Json.Serialization;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.AspNetCore.OAuth2;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(IAsyncEnumerable<RefreshToken>))]
internal partial class TokenSerializerContext : JsonSerializerContext { }