namespace NCoreUtils.OAuth2.Data;

public static class DefaultRefreshTokenNameFactory
{
    public static string GetName(Type type)
    {
        if (type == typeof(RefreshToken))
        {
            return "refreshToken";
        }
        throw new InvalidOperationException("DefaultRefreshTokenNameFactory intended to be used to configure NCoreUtils.OAuth2.Data.RefreshToken to default name.");
    }

    public static string GetName(System.Reflection.PropertyInfo property)
    {
        if (property.DeclaringType == typeof(RefreshToken))
        {
            return property.Name switch
            {
                nameof(RefreshToken.Id) => "id",
                nameof(RefreshToken.Sub) => "sub",
                nameof(RefreshToken.Issuer) => "issuer",
                nameof(RefreshToken.Email) => "email",
                nameof(RefreshToken.Username) => "username",
                nameof(RefreshToken.Scopes) => "scopes",
                nameof(RefreshToken.IssuedAt) => "issuedAt",
                nameof(RefreshToken.ExpiresAt) => "expiresAt",
                _ => throw new InvalidOperationException($"Unknown NCoreUtils.OAuth2.Data.RefreshToken property: {property.Name}."),
            };
        }
        throw new InvalidOperationException("DefaultRefreshTokenNameFactory intended to be used to configure NCoreUtils.OAuth2.Data.RefreshToken to default name.");
    }
}