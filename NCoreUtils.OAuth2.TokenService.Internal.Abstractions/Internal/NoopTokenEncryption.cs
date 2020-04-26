using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2.Internal
{
    public class NoopTokenEncryption : ITokenEncryption
    {
        public ValueTask<Token> DecryptTokenAsync(byte[] encryptedToken, CancellationToken cancellationToken = default)
        {
            if (Token.TryReadFrom(encryptedToken.AsSpan(), out var token))
            {
                return new ValueTask<Token>(token);
            }
            throw new InvalidOperationException("Unable to read token: invalid token.");
        }

        public ValueTask<byte[]> EncryptTokenAsync(Token token, CancellationToken cancellationToken = default)
        {
            return new ValueTask<byte[]>(token.ToByteArray());
        }
    }
}