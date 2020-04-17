using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.OAuth2
{
    public class TokenService : ITokenService
    {
        protected ILogger Logger { get; }

        protected ITokenServiceConfiguration Configuration { get; }

        protected ILoginProvider LoginProvider { get; }

        protected ITokenEncryption TokenEncryption { get; }

        protected ITokenRepository TokenRepository { get; }

        public TokenService(
            ILogger<TokenService> logger,
            ITokenServiceConfiguration configuration,
            ILoginProvider loginProvider,
            ITokenEncryption tokenEncryption,
            ITokenRepository tokenRepository)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            LoginProvider = loginProvider ?? throw new ArgumentNullException(nameof(loginProvider));
            TokenEncryption = tokenEncryption ?? throw new ArgumentNullException(nameof(tokenEncryption));
            TokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
        }

        protected virtual Token CreateAccessToken(DateTimeOffset now, LoginIdentity identity)
            => new Token(
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
            => new Token(
                tokenType: TokenTypes.RefreshToken,
                sub: identity.Sub,
                issuer: identity.Issuer,
                email: identity.Email,
                username: identity.Name,
                scopes: identity.Scopes.ToList(),
                issuedAt: now,
                expiresAt: now + Configuration.RefreshTokenExpiry
            );

        protected virtual async ValueTask<AccessTokenResponse> CreateTokensAndResponseAsync(LoginIdentity? identity, CancellationToken cancellationToken = default)
        {
            if (identity is null)
            {
                // FIXME: error
                throw new InvalidOperationException("WIP");
            }
            var now = DateTimeOffset.Now.Normalize();
            var refreshToken = CreateRefreshToken(now, identity);
            var accessToken = CreateAccessToken(now, identity);
            var accessTokenBase64 = await TokenEncryption.EncryptTokenToBase64StringAsync(accessToken, cancellationToken);
            var refreshTokenBase64 = await TokenEncryption.EncryptTokenToBase64StringAsync(refreshToken, cancellationToken);
            await TokenRepository.PersistRefreshTokenAsync(refreshToken, cancellationToken);
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
            var identity = await LoginProvider.ExtensionGrantAsync(type, passcode, scopes, cancellationToken);
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
                throw new AccessDeniedException("Introspected token is invalid.", exn);
            }
            if (tok is null)
            {
                throw new AccessDeniedException("Introspected token is invalid.");
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
            var identity = await LoginProvider.PasswordGrantAsync(username, password, scopes, cancellationToken);
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
            if (tok is null || tok.ExpiresAt < DateTimeOffset.Now || !await TokenRepository.CheckRefreshTokenAsync(tok, cancellationToken))
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
}