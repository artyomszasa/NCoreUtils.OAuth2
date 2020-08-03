using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.OAuth2
{
    public class CookieTokenHandler : ITokenHandler
    {
        private sealed class CustomCookieTokenHandler : CookieTokenHandler
        {
            protected override string CookieName { get; }

            public CustomCookieTokenHandler(string cookieName)
            {
                if (string.IsNullOrEmpty(cookieName))
                {
                    throw new System.ArgumentException($"'{nameof(cookieName)}' cannot be null or empty", nameof(cookieName));
                }
                CookieName = cookieName;
            }
        }

        public static CookieTokenHandler WithCustomCookieName(string cookieName)
            => new CustomCookieTokenHandler(cookieName);

        public string? CurrentToken { get; private set; }

        protected virtual string CookieName => "access_token";

        public ValueTask<string?> ReadTokenAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Cookies.TryGetValue(CookieName, out var token))
            {
                CurrentToken = token;
                return new ValueTask<string?>(token);
            }
            return default;
        }
    }
}