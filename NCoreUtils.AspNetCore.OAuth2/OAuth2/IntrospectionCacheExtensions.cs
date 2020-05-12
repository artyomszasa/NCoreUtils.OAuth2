using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2
{
    public static class IntrospectionCacheExtensions
    {
        public static async ValueTask<IntrospectionResponse> IntrospectAsync(
            this IIntrospectionCache cache,
            ITokenService service,
            string token,
            string? tokenHint = default,
            string? bearerToken = default,
            CancellationToken cancellationToken = default)
        {
            if (cache.TryGetCachedResponse(token, out var response))
            {
                return response;
            }
            response = await service.IntrospectAsync(token, tokenHint, bearerToken, cancellationToken);
            cache.Store(token, response);
            return response;
        }
    }
}