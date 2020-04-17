using System;

namespace NCoreUtils.OAuth2
{
    public interface ITokenServiceConfiguration
    {
        TimeSpan RefreshTokenExpiry { get; }

        TimeSpan AccessTokenExpiry { get; }
    }
}