using System;
using System.Runtime.Serialization;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    [Serializable]
    public class RemoteAccessDeniedException : AccessDeniedException, IRemoteException
    {
        public string Endpoint { get; }

        public override string Message
            => string.IsNullOrEmpty(Endpoint)
                ? base.Message
                : $"{base.Message} [Endpoint = {Endpoint}]";

        public RemoteAccessDeniedException(string endpoint, string message)
            : base(message)
            => Endpoint = endpoint;

        public RemoteAccessDeniedException(string endpoint, string message, Exception innerException)
            : base(message, innerException)
            => Endpoint = endpoint;

        protected RemoteAccessDeniedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => Endpoint = info.GetString(nameof(Endpoint)) ?? string.Empty;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Endpoint), Endpoint);
        }
    }
}