using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.OAuth2
{
    public interface ITokenHandler
    {
        string? CurrentToken { get; }

        ValueTask<string?> ReadTokenAsync(HttpRequest request, CancellationToken cancellationToken = default);
    }
}