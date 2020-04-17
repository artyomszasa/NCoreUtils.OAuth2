using System;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2
{
    public interface ITokenEncryption
    {
        ValueTask<EncryptTokenResult> TryEncryptTokenAsync(Token token, Memory<byte> buffer, CancellationToken cancellationToken = default);

        ValueTask<Token?> DecryptTokenAsync(ReadOnlyMemory<byte> encryptedToken, CancellationToken cancellationToken = default);
    }
}