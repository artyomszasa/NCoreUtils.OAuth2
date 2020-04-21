using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.OAuth2
{
    public class CookieTokenHandler : ITokenHandler
    {
        public ValueTask<string?> ReadTokenAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Cookies.TryGetValue("access_token", out var token))
            {
                return new ValueTask<string?>(token);
            }
            return default;
        }
    }
}