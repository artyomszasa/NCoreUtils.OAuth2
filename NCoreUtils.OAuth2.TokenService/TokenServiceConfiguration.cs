using System;

namespace NCoreUtils.OAuth2
{
    public class TokenServiceConfiguration : ITokenServiceConfiguration
    {
        public TimeSpan RefreshTokenExpiry { get; set; }

        public TimeSpan AccessTokenExpiry { get; set; }
    }
}