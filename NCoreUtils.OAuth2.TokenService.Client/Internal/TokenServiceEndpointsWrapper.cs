using System;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2.Internal;

public class TokenServiceEndpointsWrapper(ITokenServiceEndpoints endpoints) : ITokenService
{
    private readonly ITokenServiceEndpoints _endpoints = endpoints ?? throw new ArgumentNullException(nameof(endpoints));

    public ValueTask<AccessTokenResponse> ExtensionGrantAsync(string type, string passcode, ScopeCollection scopes, CancellationToken cancellationToken = default)
        => new(_endpoints.TokenAsync(
            grantType: type,
            passcode: passcode,
            username: default,
            password: default,
            refreshToken: default,
            scope: scopes,
            cancellationToken: cancellationToken
        ));

    public ValueTask<IntrospectionResponse> IntrospectAsync(string token, string? tokenTypeHint = null, string? bearerToken = null, CancellationToken cancellationToken = default)
        => _endpoints.IntrospectAsync(
            token: token,
            tokenTypeHint: tokenTypeHint,
            bearerToken: bearerToken,
            cancellationToken: cancellationToken
        );

    public ValueTask<AccessTokenResponse> PasswordGrantAsync(string username, string password, ScopeCollection scopes, CancellationToken cancellationToken = default)
        => new(_endpoints.TokenAsync(
            grantType: "password",
            passcode: default,
            username: username,
            password: password,
            refreshToken: default,
            scope: scopes,
            cancellationToken: cancellationToken
        ));

    public ValueTask<AccessTokenResponse> RefreshTokenAsync(string refreshToken, ScopeCollection scopes, CancellationToken cancellationToken = default)
        => new(_endpoints.TokenAsync(
            grantType: "refresh_token",
            passcode: default,
            username: default,
            password: default,
            refreshToken: refreshToken,
            scope: scopes,
            cancellationToken: cancellationToken
        ));
}