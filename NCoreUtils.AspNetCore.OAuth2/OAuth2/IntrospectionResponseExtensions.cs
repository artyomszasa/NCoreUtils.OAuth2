using System.Collections.Generic;
using System.Security.Claims;

namespace NCoreUtils.OAuth2
{
    public static class IntrospectionResponseExtensions
    {
        public static IEnumerable<Claim> ReadClaims(this IntrospectionResponse data)
        {
            yield return new Claim(ClaimTypes.Sid, data.Sub, ClaimValueTypes.String, data.Issuer);
            if (null != data.Username)
            {
                yield return new Claim(ClaimTypes.Name, data.Username, ClaimValueTypes.String, data.Issuer);
            }
            if (null != data.Email)
            {
                yield return new Claim(ClaimTypes.Email, data.Email, ClaimValueTypes.Email, data.Issuer);
            }
            if (data.ExpiresAt.HasValue)
            {
                yield return new Claim(ClaimTypes.Expiration, data.ExpiresAt.Value.ToString("u"), ClaimValueTypes.DateTime, data.Issuer);
            }
            if (data.IssuedAt.HasValue)
            {
                yield return new Claim(OAuth2ClaimTypes.IssuedAt, data.IssuedAt.Value.ToString("u"), ClaimValueTypes.DateTime, data.Issuer);
            }
            if (null != data.Issuer)
            {
                yield return new Claim(OAuth2ClaimTypes.Ownership, data.Issuer, ClaimValueTypes.String, data.Issuer);
            }
            foreach (var scope in data.Scope)
            {
                yield return new Claim(ClaimTypes.Role, scope, ClaimValueTypes.String, data.Issuer);
            }
        }
    }
}