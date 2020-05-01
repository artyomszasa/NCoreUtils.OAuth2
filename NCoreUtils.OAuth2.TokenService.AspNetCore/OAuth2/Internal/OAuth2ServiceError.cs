using System.Text.Json;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class OAuth2ServiceError : CustomServiceError
    {
        private static readonly JsonError _jsonError = new JsonError(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { ErrorResponseConverter.Instance }
        });

        public override ErrorSerializer CreateSerializer(MethodDescriptor method)
            => new OAuth2ErrorSerializer(_jsonError);
    }
}