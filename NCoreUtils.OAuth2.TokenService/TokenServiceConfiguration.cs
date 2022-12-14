using System;

namespace NCoreUtils.OAuth2
{
    public class TokenServiceConfiguration : ITokenServiceConfiguration
    {
        public TimeSpan RefreshTokenExpiry { get; }

        public TimeSpan AccessTokenExpiry { get; }

        public TokenServiceConfiguration(TimeSpan refreshTokenExpiry, TimeSpan accessTokenExpiry)
        {
            RefreshTokenExpiry = refreshTokenExpiry;
            AccessTokenExpiry = accessTokenExpiry;
        }
    }
}