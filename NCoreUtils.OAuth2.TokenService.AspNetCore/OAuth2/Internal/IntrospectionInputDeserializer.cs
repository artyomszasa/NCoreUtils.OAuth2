using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal
{
    public class IntrospectionInputDeserializer : InputDeserializer
    {
        public static IntrospectionInputDeserializer Instance { get; } = new IntrospectionInputDeserializer();

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

        public override async ValueTask<object[]> ReadRequestAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            var form = await request.ReadFormAsync(cancellationToken);
            var headers = request.Headers;
            return new object[]
            {
                // token
                form.TryGetFirstValue("token", out var token) ? token : default!,
                // tokenTypeHint
                form.TryGetFirstValue("token_type_hint", out var tokenTypeHint) ? tokenTypeHint : default!,
                // bearerToken
                GetBearerToken(request)!
            };
        }
    }
}