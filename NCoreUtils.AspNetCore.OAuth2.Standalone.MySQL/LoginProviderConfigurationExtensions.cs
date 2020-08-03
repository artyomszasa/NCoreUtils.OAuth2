using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace NCoreUtils.AspNetCore.OAuth2
{
    internal static class LoginProviderConfigurationExtensions
    {
        internal static bool TryChoose(
            this List<LoginProviderConfiguration> configurations,
            HttpContext context,
            [NotNullWhen(true)] out LoginProviderConfiguration? match)
        {
            foreach (var configuration in configurations)
            {
                if (configuration.Matches(context.Request))
                {
                    match = configuration;
                    return true;
                }
            }
            match = default;
            return false;
        }
    }
}