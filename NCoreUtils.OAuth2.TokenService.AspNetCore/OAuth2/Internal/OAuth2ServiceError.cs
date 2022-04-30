using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class OAuth2ServiceError : CustomServiceError
    {
        private static readonly JsonError _jsonError = JsonError.Json<TokenServiceJsonSerializerContext>();

        public override ErrorSerializer CreateSerializer(MethodDescriptor method)
            => new OAuth2ErrorSerializer(_jsonError);
    }
}