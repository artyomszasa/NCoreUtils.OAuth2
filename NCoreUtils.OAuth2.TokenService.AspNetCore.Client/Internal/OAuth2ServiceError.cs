using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class OAuth2ServiceError : CustomClientError
    {
        public override ErrorDeserializer CreateDeserializer(MethodDescriptor method)
            => OAuth2ErrorDeserializer.Singleton;
    }
}