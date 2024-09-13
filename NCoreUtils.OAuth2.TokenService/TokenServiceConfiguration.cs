using System;

namespace NCoreUtils.OAuth2;

public class TokenServiceConfiguration(TimeSpan refreshTokenExpiry, TimeSpan accessTokenExpiry) : ITokenServiceConfiguration
{
    public static readonly TimeSpan DefaultRefreshTokenExpiry = TimeSpan.FromDays(30);

    public static readonly TimeSpan DefaultAccessTokenExpiry = TimeSpan.FromMinutes(15);

    public TimeSpan RefreshTokenExpiry { get; } = refreshTokenExpiry;

    public TimeSpan AccessTokenExpiry { get; } = accessTokenExpiry;
}