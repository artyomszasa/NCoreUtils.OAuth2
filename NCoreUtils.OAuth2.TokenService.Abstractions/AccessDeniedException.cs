using System;
using System.Runtime.Serialization;

namespace NCoreUtils.OAuth2;

#if !NET8_0_OR_GREATER
[Serializable]
#endif
public class AccessDeniedException : TokenServiceException
{
    public AccessDeniedException(string message, Exception innerException)
        : base(TokenServiceErrorCodes.AccessDenied, 401, message, innerException)
    { }

    public AccessDeniedException(string message)
        : base(TokenServiceErrorCodes.AccessDenied, 401, message)
    { }

#if !NET8_0_OR_GREATER
    protected AccessDeniedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
#endif
}