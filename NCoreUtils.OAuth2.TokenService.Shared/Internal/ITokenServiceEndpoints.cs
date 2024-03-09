using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2.Internal;

/// <summary>
/// <c>Proto</c> interface that handles all <c>token</c> requests within a single method.
/// </summary>
public interface ITokenServiceEndpoints
{
    Task<AccessTokenResponse> TokenAsync(
        string grantType,
        string? passcode,
        string? username,
        string? password,
        string? refreshToken,
        ScopeCollection scope,
        CancellationToken cancellationToken);

    ValueTask<IntrospectionResponse> IntrospectAsync(
        string token,
        string? tokenTypeHint = default,
        string? bearerToken = default,
        CancellationToken cancellationToken = default);

}