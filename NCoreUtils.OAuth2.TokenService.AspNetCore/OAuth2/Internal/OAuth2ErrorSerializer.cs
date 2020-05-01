using System;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class OAuth2ErrorSerializer : JsonErrorSerializer
    {
        public OAuth2ErrorSerializer(JsonError jsonError) : base(jsonError) { }

        protected override IErrorDescription GetErrorDescription(Exception exn)
            => exn switch
            {
                TokenServiceException e => new ErrorResponse(e.ErrorCode, e.Message),
                _ => new ErrorResponse(TokenServiceErrorCodes.InternalError, exn.Message)
            };
    }
}