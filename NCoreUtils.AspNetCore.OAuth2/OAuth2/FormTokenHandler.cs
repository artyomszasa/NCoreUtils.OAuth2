using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.OAuth2
{
    public class FormTokenHandler : ITokenHandler
    {
        private static bool IsFormCompatibleContentType(string contentType)
            => contentType.StartsWith("application/x-www-form-urlencoded", true, CultureInfo.InvariantCulture)
                || contentType.StartsWith("multipart/form-data", true, CultureInfo.InvariantCulture);

        public async ValueTask<string?> ReadTokenAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            if ((request.Method == "POST" || request.Method == "PUT") && IsFormCompatibleContentType(request.ContentType))
            {
                var form = await request.ReadFormAsync(cancellationToken);
                if (form.TryGetValue("access_token", out var values) && values.Count > 0)
                {
                    return values[0];
                }
            }
            return default;
        }
    }
}