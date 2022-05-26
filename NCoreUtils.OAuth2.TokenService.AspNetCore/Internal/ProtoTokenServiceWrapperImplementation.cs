using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Internal;
using NCoreUtils.Proto;
using NCoreUtils.Proto.Internal;

namespace NCoreUtils.AspNetCore.Internal;

public partial class ProtoTokenServiceWrapperImplementation
{
    private static string? GetBearerToken(StringValues vs)
    {
        foreach (var v in vs)
        {
            if (v is not null && v.StartsWith("bearer ", StringComparison.InvariantCultureIgnoreCase))
            {
                var tokenIndex = 7;
                while (v.Length > tokenIndex && char.IsWhiteSpace(v[tokenIndex]))
                {
                    ++tokenIndex;
                }
                return v.Substring(tokenIndex);
            }
        }
        return default;
    }

    private async ValueTask<DtoITokenServiceEndpointsIntrospectArgs> ReadIntrospectRequestAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var data = await request.ReadFormAsync(cancellationToken);
        return new DtoITokenServiceEndpointsIntrospectArgs(
            token: ReadArgument<string>(data["token"]),
            tokenTypeHint: ReadArgument<string?>(data["token_type_hint"]),
#if NET6_0_OR_GREATER
            bearerToken: GetBearerToken(request.Headers.Authorization)
#else
            bearerToken: request.Headers.TryGetValue("Authorization", out var vs) ? GetBearerToken(vs) : default
#endif
        );
    }

    protected override Task WriteErrorAsync(
        ILogger logger,
        HttpResponse response,
        Exception exn,
        CancellationToken cancellationToken)
    {
        if (exn is TokenServiceException text)
        {
            response.StatusCode = text.DesiredStatusCode;
            return JsonSerializer.SerializeAsync(
                response.Body,
                new ErrorDescriptor(text.ErrorCode, exn.Message),
                ErrorDescriptorSerializerContext.Default.ErrorDescriptor,
                cancellationToken
            );
        }
        return base.WriteErrorAsync(logger, response, exn, cancellationToken);
    }
}