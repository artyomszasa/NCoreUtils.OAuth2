using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.OAuth2
{
    public class QueryTokenHandler : ITokenHandler
    {
        public string? CurrentToken { get; set; }

        public ValueTask<string?> ReadTokenAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Query.TryGetValue("access_token", out var values) && values.Count > 0)
            {
                CurrentToken = values[0];
                return new ValueTask<string?>(values[0]);
            }
            return default;
        }
    }
}