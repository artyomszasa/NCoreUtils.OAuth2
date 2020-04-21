using Microsoft.AspNetCore.Authentication;
using NCoreUtils.OAuth2;

namespace NCoreUtils.AspNetCore
{
    public static class AuthenticationBuilderOAuth2Extensions
    {
        public static AuthenticationBuilder AddRemoteOAuth2AuthenticationScheme(this AuthenticationBuilder builder)
        {
            builder.AddScheme<OAuth2AuthenticationSchemeOptions,OAuth2AuthenticationHandler>(OAuth2AuthenticationSchemeOptions.Name, _ => { });
            return builder;
        }
    }
}