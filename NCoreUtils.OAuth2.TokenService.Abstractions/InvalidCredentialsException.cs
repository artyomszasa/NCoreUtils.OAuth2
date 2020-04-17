using System;
using System.Runtime.Serialization;

namespace NCoreUtils.OAuth2
{
    [Serializable]
    public class InvalidCredentialsException : TokenServiceException
    {
        public InvalidCredentialsException(string message, Exception innerException)
            : base(TokenServiceErrorCodes.InvalidCredentials, 401, message, innerException)
        { }

        public InvalidCredentialsException(string message)
            : base(TokenServiceErrorCodes.InvalidCredentials, 401, message)
        { }

        protected InvalidCredentialsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}