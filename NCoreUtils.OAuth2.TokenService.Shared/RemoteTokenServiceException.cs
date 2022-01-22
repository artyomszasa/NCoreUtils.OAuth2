using System;
using System.Runtime.Serialization;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    [Serializable]
    public class RemoteTokenServiceException : TokenServiceException, IRemoteException
    {
        public string Endpoint { get; }

        public override string Message
            => string.IsNullOrEmpty(Endpoint)
                ? base.Message
                : $"{base.Message} [Endpoint = {Endpoint}]";


        public RemoteTokenServiceException(string endpoint, string errorCode, int desiredStatusCode, string message)
            : base(errorCode, desiredStatusCode, message)
            => Endpoint = endpoint;

        public RemoteTokenServiceException(
            string endpoint,
            string errorCode,
            int desiredStatusCode,
            string message,
            Exception innerException)
            : base(errorCode, desiredStatusCode, message, innerException)
            => Endpoint = endpoint;

        protected RemoteTokenServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => Endpoint = info.GetString(nameof(Endpoint)) ?? string.Empty;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Endpoint), Endpoint);
        }
    }
}