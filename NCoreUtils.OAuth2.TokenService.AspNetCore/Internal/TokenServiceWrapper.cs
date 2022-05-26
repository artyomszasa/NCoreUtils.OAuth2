using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.OAuth2;
using NCoreUtils.OAuth2.Internal;
using NCoreUtils.Proto;

namespace NCoreUtils.AspNetCore.Internal;

[ProtoService(typeof(TokenServiceEndpointsInfo), typeof(TokenServiceSerializerContext), ImplementationFactory = typeof(TokenServiceWrapper), Path = "")]
public class TokenServiceWrapper : ITokenServiceEndpoints
{
    public static ProtoTokenServiceWrapperImplementation CreateService(IServiceProvider serviceProvider)
        => new(new TokenServiceWrapper(serviceProvider.GetRequiredService<ITokenService>()));

    private readonly ITokenService _tokenService;

    public TokenServiceWrapper(ITokenService tokenService)
        => _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

    public ValueTask<IntrospectionResponse> IntrospectAsync(
        string token,
        string? tokenTypeHint = null,
        string? bearerToken = null,
        CancellationToken cancellationToken = default)
        => _tokenService.IntrospectAsync(token, tokenTypeHint, bearerToken, cancellationToken);

    public Task<AccessTokenResponse> TokenAsync(
        string grantType,
        string? passcode,
        string? username,
        string? password,
        string? refreshToken,
        ScopeCollection scope,
        CancellationToken cancellationToken)
        => grantType switch
        {
            "password" => _tokenService.PasswordGrantAsync(username!, password!, scope, cancellationToken).AsTask(),
            "refresh_token" => _tokenService.RefreshTokenAsync(refreshToken!, scope, cancellationToken).AsTask(),
            _ => _tokenService.ExtensionGrantAsync(grantType, passcode!, scope, cancellationToken).AsTask()
        };
}