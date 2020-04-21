using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.OAuth2
{
    public class CompositeTokenHandler : ITokenHandler
    {
        public IReadOnlyList<ITokenHandler> Handlers { get; }

        public CompositeTokenHandler(IReadOnlyList<ITokenHandler> handlers)
            => Handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));

        public async ValueTask<string?> ReadTokenAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            foreach (var handler in Handlers)
            {
                var token = await handler.ReadTokenAsync(request, cancellationToken);
                if (null != token)
                {
                    return token;
                }
            }
            return default;
        }
    }
}