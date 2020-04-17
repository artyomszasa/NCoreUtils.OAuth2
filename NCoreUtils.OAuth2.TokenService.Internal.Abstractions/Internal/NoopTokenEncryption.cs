using System;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2.Internal
{
    public class NoopTokenEncryption : ITokenEncryption
    {
        public ValueTask<Token?> DecryptTokenAsync(ReadOnlyMemory<byte> encryptedToken, CancellationToken cancellationToken = default)
        {
            if (Token.TryReadFrom(encryptedToken.Span, out var token))
            {
                return new ValueTask<Token?>(token);
            }
            return default;
        }

        public ValueTask<EncryptTokenResult> TryEncryptTokenAsync(Token token, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var success = token.TryWriteTo(buffer.Span, out var size);
            return new ValueTask<EncryptTokenResult>(new EncryptTokenResult(success, size));
        }
    }
}