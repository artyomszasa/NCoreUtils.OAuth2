using System.Text.Json;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class OAuth2ClientError : CustomClientError
    {
        private static readonly JsonError _jsonError = new JsonError(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { ErrorResponseConverter.Instance }
        });

        public override ErrorDeserializer CreateDeserializer(MethodDescriptor method)
            => new OAuth2ErrorDeserializer(_jsonError);
    }
}