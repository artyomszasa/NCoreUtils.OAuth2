using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.OAuth2
{
    public class BearerTokenHandler : ITokenHandler
    {
        private static string? GetBearerToken(HttpRequest request)
        {
            var headers = request.Headers;
            var auth = headers.TryGetValue("Authorization", out var values) && values.Count > 0 ? values[0] : default;
            if (!string.IsNullOrEmpty(auth) && auth.StartsWith("bearer "))
            {
                var tokenIndex = 7;
                while (auth.Length > tokenIndex && char.IsWhiteSpace(auth[tokenIndex]))
                {
                    ++tokenIndex;
                }
                return auth.Substring(tokenIndex);
            }
            return default;
        }

        public ValueTask<string?> ReadTokenAsync(HttpRequest request, CancellationToken cancellationToken = default)
            => new ValueTask<string?>(GetBearerToken(request));
    }
}