using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.OAuth2
{
    public class IntrospectionNoCache : IIntrospectionCache
    {
        public void Store(string token, IntrospectionResponse response) { }

        public bool TryGetCachedResponse(string token, [NotNullWhen(true)] out IntrospectionResponse? response)
        {
            response = default;
            return false;
        }
    }
}