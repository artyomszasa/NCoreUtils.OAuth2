using System;
using System.Runtime.Serialization;

namespace NCoreUtils.OAuth2
{
    [Serializable]
    public class AccessDeniedException : TokenServiceException
    {
        public AccessDeniedException(string message, Exception innerException)
            : base(TokenServiceErrorCodes.AccessDenied, 401, message, innerException)
        { }

        public AccessDeniedException(string message)
            : base(TokenServiceErrorCodes.AccessDenied, 401, message)
        { }

        protected AccessDeniedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}