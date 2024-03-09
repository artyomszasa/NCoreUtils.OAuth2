using System;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2;

public interface ITokenEncryption
{
    ValueTask<byte[]> EncryptTokenAsync(Token token, CancellationToken cancellationToken = default);

    ValueTask<Token> DecryptTokenAsync(byte[] encryptedToken, int offset, int count, CancellationToken cancellationToken = default);

    ValueTask<Token> DecryptTokenAsync(byte[] encryptedToken, CancellationToken cancellationToken = default)
        => DecryptTokenAsync(encryptedToken, 0, encryptedToken.Length, cancellationToken);
}