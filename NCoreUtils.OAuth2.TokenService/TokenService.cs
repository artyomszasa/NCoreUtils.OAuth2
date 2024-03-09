using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2;

public class TokenService(
    ILogger<TokenService> logger,
    ITokenServiceConfiguration configuration,
    ILoginProvider loginProvider,
    ITokenEncryption tokenEncryption,
    LazyService<ITokenRepository> tokenRepository) : ITokenService
{
    protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

    protected ITokenServiceConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));

    protected ILoginProvider LoginProvider { get; } = loginProvider ?? throw new ArgumentNullException(nameof(loginProvider));

    protected ITokenEncryption TokenEncryption { get; } = tokenEncryption ?? throw new ArgumentNullException(nameof(tokenEncryption));

    protected LazyService<ITokenRepository> TokenRepository { get; } = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));

    protected virtual Token CreateAccessToken(DateTimeOffset now, LoginIdentity identity)
        => new(
            tokenType: TokenTypes.AccessToken,
            sub: identity.Sub,
            issuer: identity.Issuer,
            email: identity.Email,
            username: identity.Name,
            scopes: identity.Scopes.ToList(),
            issuedAt: now,
            expiresAt: now + Configuration.AccessTokenExpiry
        );

    protected virtual Token CreateRefreshToken(DateTimeOffset now, LoginIdentity identity)
        => new(
            tokenType: TokenTypes.RefreshToken,
            sub: identity.Sub,
            issuer: identity.Issuer,
            email: identity.Email,
            username: identity.Name,
            scopes: identity.Scopes.ToList(),
            issuedAt: now,
            expiresAt: now + Configuration.RefreshTokenExpiry
        );

    protected virtual async ValueTask<AccessTokenResponse> CreateTokensAndResponseAsync(LoginIdentity identity, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.Now.Normalize();
        var refreshToken = CreateRefreshToken(now, identity);
        var accessToken = CreateAccessToken(now, identity);
        var accessTokenBase64 = await TokenEncryption.EncryptTokenToBase64StringAsync(accessToken, cancellationToken);
        var refreshTokenBase64 = await TokenEncryption.EncryptTokenToBase64StringAsync(refreshToken, cancellationToken);
        await TokenRepository.Instance.PersistRefreshTokenAsync(refreshToken, cancellationToken);
        return new AccessTokenResponse(
            accessToken: accessTokenBase64,
            tokenType: "bearer",
            expiresIn: accessToken.ExpiresAt - DateTimeOffset.Now, // assume some time may has been expired since generation
            refreshToken: refreshTokenBase64,
            scope: identity.Scopes
        );
    }


    public async ValueTask<AccessTokenResponse> ExtensionGrantAsync(string type, string passcode, ScopeCollection scopes, CancellationToken cancellationToken = default)
    {
        var identity = await LoginProvider.ExtensionGrantAsync(type, passcode, scopes, cancellationToken)
            ?? throw new InvalidCredentialsException("Specified credentials are not valid.");
        return await CreateTokensAndResponseAsync(identity, cancellationToken);
    }

    public async ValueTask<IntrospectionResponse> IntrospectAsync(string token, string? tokenTypeHint = null, string? bearerToken = null, CancellationToken cancellationToken = default)
    {
        Token? tok;
        try
        {
            tok = await TokenEncryption.DecryptTokenFromBase64StringAsync(token, cancellationToken);
        }
        catch (Exception exn)
        {
            Logger.LogDebug(exn, "Introspected token is invalid.");
            throw new AccessDeniedException("Introspected token is invalid.", exn);
        }
        if (tok is null)
        {
            Logger.LogDebug("Introspected token is invalid.");
            throw new AccessDeniedException("Introspected token is invalid.");
        }
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.Log(LogLevel.Debug, default, new TokenData(tok), default, (data, _) => $"Inspecting: {data}");
        }
        if (tok.ExpiresAt < DateTimeOffset.Now)
        {
            return IntrospectionResponse.Inactive;
        }
        return new IntrospectionResponse(
            active: true,
            scope: new ScopeCollection(tok.Scopes),
            clientId: default,
            email: tok.Email,
            username: tok.Username,
            tokenType: tok.TokenType,
            expiresAt: tok.ExpiresAt,
            issuedAt: tok.IssuedAt,
            notBefore: default,
            sub: tok.Sub,
            issuer: tok.Issuer
        );
    }

    public async ValueTask<AccessTokenResponse> PasswordGrantAsync(string username, string password, ScopeCollection scopes, CancellationToken cancellationToken = default)
    {
        var identity = await LoginProvider.PasswordGrantAsync(username, password, scopes, cancellationToken)
            ?? throw new InvalidCredentialsException("Invalid username or password.");
        return await CreateTokensAndResponseAsync(identity, cancellationToken);
    }

    public async ValueTask<AccessTokenResponse> RefreshTokenAsync(string refreshToken, ScopeCollection scopes, CancellationToken cancellationToken = default)
    {
        Token? tok;
        try
        {
            tok = await TokenEncryption.DecryptTokenFromBase64StringAsync(refreshToken, cancellationToken);
        }
        catch (Exception exn)
        {
            throw new AccessDeniedException("Refresh token is invalid.", exn);
        }
        if (tok is null || tok.ExpiresAt < DateTimeOffset.Now || !await TokenRepository.Instance.CheckRefreshTokenAsync(tok, cancellationToken))
        {
            throw new AccessDeniedException("Refresh token is invalid.");
        }
        var now = DateTimeOffset.Now.Normalize();
        var accessToken = new Token(
            tokenType: TokenTypes.AccessToken,
            sub: tok.Sub,
            issuer: tok.Issuer,
            email: tok.Email,
            username: tok.Username,
            scopes: tok.Scopes,
            issuedAt: now,
            expiresAt: now + Configuration.AccessTokenExpiry
        );
        var accessTokenBase64 = await TokenEncryption.EncryptTokenToBase64StringAsync(accessToken, cancellationToken);
        return new AccessTokenResponse(
            accessToken: accessTokenBase64,
            tokenType: "bearer",
            expiresIn: accessToken.ExpiresAt - DateTimeOffset.Now, // assume some time may has been expired since generation
            refreshToken: default,
            scope: new ScopeCollection(accessToken.Scopes)
        );
    }
}