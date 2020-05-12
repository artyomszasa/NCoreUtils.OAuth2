using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;

namespace NCoreUtils.OAuth2
{
    public class IntrospectionMemoryCache : IIntrospectionCache
    {
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public void Store(string token, IntrospectionResponse response)
        {
            if (response.ExpiresAt.HasValue)
            {
                _cache.Set(token, response, response.ExpiresAt.Value);
            }
        }

        public bool TryGetCachedResponse(string token, [NotNullWhen(true)] out IntrospectionResponse? response)
            => _cache.TryGetValue(token, out response!);
    }
}