using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2
{
    public interface ITokenService
    {
        ValueTask<AccessTokenResponse> ExtensionGrantAsync(
            string type,
            string passcode,
            ScopeCollection scopes,
            CancellationToken cancellationToken = default);

        ValueTask<IntrospectionResponse> IntrospectAsync(
            string token,
            string? tokenTypeHint = default,
            string? bearerToken = default,
            CancellationToken cancellationToken = default);

        ValueTask<AccessTokenResponse> PasswordGrantAsync(
            string username,
            string password,
            ScopeCollection scopes,
            CancellationToken cancellationToken = default);

        ValueTask<AccessTokenResponse> RefreshTokenAsync(
            string refreshToken,
            ScopeCollection scopes,
            CancellationToken cancellationToken = default);
    }
}