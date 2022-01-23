using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using NCoreUtils.OAuth2;

namespace NCoreUtils.AspNetCore
{
    public static class AuthenticationBuilderOAuth2Extensions
    {
        public static AuthenticationBuilder AddRemoteOAuth2AuthenticationScheme(this AuthenticationBuilder builder)
        {
            builder.AddScheme<OAuth2AuthenticationSchemeOptions, OAuth2AuthenticationHandler>(OAuth2AuthenticationSchemeOptions.Name, _ => { });
            return builder;
        }

        public static AuthenticationBuilder AddCustomRemoteOAuth2AuthenticationScheme<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TAuthenticationHandler>(
            this AuthenticationBuilder builder)
            where TAuthenticationHandler : OAuth2AuthenticationHandler
        {
            builder.AddScheme<OAuth2AuthenticationSchemeOptions, TAuthenticationHandler>(OAuth2AuthenticationSchemeOptions.Name, _ => { });
            return builder;
        }
    }
}