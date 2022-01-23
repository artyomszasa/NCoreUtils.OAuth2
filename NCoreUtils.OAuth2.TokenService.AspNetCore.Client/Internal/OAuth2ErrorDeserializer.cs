using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.OAuth2.Internal;

public class OAuth2ErrorDeserializer : ErrorDeserializer
{
    public static OAuth2ErrorDeserializer Singleton { get; } = new();

    public override async ValueTask<IErrorDescription> ReadResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(
#if !NETSTANDARD2_1
            cancellationToken
#endif
        );
        return (await JsonSerializer.DeserializeAsync(
            stream,
            TokenServiceJsonSerializerContext.Default.ErrorResponse,
            cancellationToken
        ))!;
    }
}