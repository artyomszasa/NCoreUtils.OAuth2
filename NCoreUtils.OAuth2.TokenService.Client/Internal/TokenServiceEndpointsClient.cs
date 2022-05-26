using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Proto;

namespace NCoreUtils.OAuth2.Internal;

[ProtoClient(typeof(TokenServiceEndpointsInfo), typeof(TokenServiceSerializerContext), Path = "")]
public partial class TokenServiceEndpointsClient
{
    private HttpRequestMessage CreateIntrospectRequest(string token, string? tokenTypeHint, string? bearerToken)
    {
        var data = new Dictionary<string, string>(2)
        {
            { "token", token }
        };
        if (!string.IsNullOrEmpty(tokenTypeHint))
        {
            data.Add("token_type_hint", tokenTypeHint);
        }
        var request = new HttpRequestMessage(HttpMethod.Post, GetCachedMethodPath(Methods.Introspect))
        {
            Content = new FormUrlEncodedContent(data)
        };
        if (!string.IsNullOrEmpty(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
        }
        return request;
    }

    protected override async ValueTask HandleErrors(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }
        Exception exnToThrow;
        try
        {
            var error = await response.Content
                .ReadFromJsonAsync(ErrorResponseSerializerContext.Default.ErrorResponse, cancellationToken)
                .ConfigureAwait(false);
            if (error is null)
            {
                exnToThrow = new ProtoException("generic_error", $"Remote server responded with {response.StatusCode} without content.");
            }
            else
            {
                exnToThrow = error.ToException(GetEndpoint(response));
            }
        }
        catch (Exception exn)
        {
            throw new ProtoException("generic_error", "Unable to read error response.", exn);
        }
        throw exnToThrow;

        static string GetEndpoint(HttpResponseMessage response)
            => response.RequestMessage?.RequestUri?.AbsoluteUri ?? string.Empty;
    }
}