using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.OAuth2
{
    public interface IIntrospectionCache
    {
        void Store(string token, IntrospectionResponse response);

        bool TryGetCachedResponse(string token, [NotNullWhen(true)] out IntrospectionResponse? response);
    }
}