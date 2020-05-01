using System;
using System.Runtime.Serialization;
using NCoreUtils.AspNetCore;

namespace NCoreUtils.OAuth2
{
    [Serializable]
    public class TokenServiceException : Exception, IStatusCodeResponse
    {
        int IStatusCodeResponse.StatusCode => DesiredStatusCode;

        public string ErrorCode { get; }

        public int DesiredStatusCode { get; }

        public TokenServiceException(string errorCode, int desiredStatusCode, string message, Exception innerException)
            : base(message, innerException)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                throw new ArgumentException("Error code must be non-empty string", nameof(errorCode));
            }
            ErrorCode = errorCode;
            DesiredStatusCode = desiredStatusCode;
        }

        public TokenServiceException(string errorCode, int desiredStatusCode, string message)
            : base(message)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                throw new ArgumentException("Error code must be non-empty string", nameof(errorCode));
            }
            ErrorCode = errorCode;
            DesiredStatusCode = desiredStatusCode;
        }

        protected TokenServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetString(nameof(ErrorCode));
            DesiredStatusCode = info.GetInt32(nameof(DesiredStatusCode));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorCode), ErrorCode);
            info.AddValue(nameof(DesiredStatusCode), DesiredStatusCode);
        }
    }
}