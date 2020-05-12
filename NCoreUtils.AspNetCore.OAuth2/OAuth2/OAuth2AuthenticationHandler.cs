using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NCoreUtils.OAuth2
{
    public class OAuth2AuthenticationHandler : AuthenticationHandler<OAuth2AuthenticationSchemeOptions>
    {
        public ITokenHandler TokenHandler { get; }

        public ITokenService TokenService { get; }

        public IIntrospectionCache Cache { get; }

        public OAuth2AuthenticationHandler(
            IOptionsMonitor<OAuth2AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ITokenHandler tokenHandler,
            ITokenService tokenService,
            IIntrospectionCache? introspectionCache = default)
            : base(options, logger, encoder, clock)
        {
            TokenHandler = tokenHandler ?? throw new ArgumentNullException(nameof(tokenHandler));
            TokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            Cache = introspectionCache ?? new IntrospectionNoCache();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = await TokenHandler.ReadTokenAsync(Context.Request, Context.RequestAborted);
            if (token is null)
            {
                return AuthenticateResult.NoResult();
            }
            IntrospectionResponse data;
            try
            {
                data = await Cache.IntrospectAsync(TokenService, token, cancellationToken: Context.RequestAborted);
            }
            catch (Exception exn)
            {
                return AuthenticateResult.Fail(exn);
            }
            if (!data.Active)
            {
                return AuthenticateResult.Fail("Token is not active.");
            }
            var identity = new ClaimsIdentity(data.ReadClaims(), OAuth2AuthenticationSchemeOptions.Name, ClaimTypes.Name, ClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = data.ExpiresAt,
                    IssuedUtc = data.IssuedAt
                },
                OAuth2AuthenticationSchemeOptions.Name
            ));
        }
    }
}