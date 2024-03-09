namespace NCoreUtils.OAuth2.Internal;

public class TokenData(Token token)
{
    private readonly Token _token = token;

    public override string ToString()
        => $"Token[Type = {_token.TokenType}, Sub = {_token.Sub}, Email = {_token.Email}, ExpiresAt = {_token.ExpiresAt}, IssuedAt = {_token.IssuedAt}, Issuer = {_token.Issuer}]";
}