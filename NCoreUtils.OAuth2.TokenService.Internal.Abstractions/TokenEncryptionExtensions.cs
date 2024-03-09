using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2;

public static class TokenEncryptionExtensions
{
    public static ValueTask<Token> DecryptTokenFromBase64StringAsync(
        this ITokenEncryption encryption,
        string base64string,
        CancellationToken cancellationToken = default)
    {
        var data = Convert.FromBase64String(base64string);
        return encryption.DecryptTokenAsync(data, cancellationToken);
    }

    public static async ValueTask<string> EncryptTokenToBase64StringAsync(
        this ITokenEncryption encryption,
        Token token,
        CancellationToken cancellationToken = default)
    {
        var data = await encryption.EncryptTokenAsync(token, cancellationToken);
        return Convert.ToBase64String(data);
    }
}