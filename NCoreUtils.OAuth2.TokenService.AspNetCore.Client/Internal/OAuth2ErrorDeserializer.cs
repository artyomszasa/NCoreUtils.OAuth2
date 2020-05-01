using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class OAuth2ErrorDeserializer : JsonErrorDeserializer<ErrorResponse>
    {
        public OAuth2ErrorDeserializer(JsonError error) : base(error) { }
    }
}