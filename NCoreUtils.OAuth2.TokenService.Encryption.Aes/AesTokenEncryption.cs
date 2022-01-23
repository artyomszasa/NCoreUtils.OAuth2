using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2
{
    public sealed partial class AesTokenEncryption : ITokenEncryption, IDisposable
    {
        private readonly Aes _alg;

        private readonly EncryptorPool _encryptorPool;

        private readonly DecryptorPool _decryptorPool;

        private int _isDisposed;

        private bool IsDisposed
        {
            get => 0 != Interlocked.CompareExchange(ref _isDisposed, 0, 0);
        }

        public AesTokenEncryption(AesTokenEncryptionConfiguration configuration)
        {
            _alg = Aes.Create();
            _alg.KeySize = configuration.KeyValue.Length * 8;
            _alg.Key = configuration.KeyValue;
            _alg.IV = configuration.IVValue;
            _encryptorPool = new EncryptorPool(_alg);
            _decryptorPool = new DecryptorPool(_alg);
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(AesTokenEncryption));
            }
        }

        public ValueTask<Token> DecryptTokenAsync(byte[] encryptedToken, int offset, int count, CancellationToken cancellationToken = default)
        {
            var decryptor = _decryptorPool.Rent();
            try
            {
                var data = decryptor.TransformFinalBlock(encryptedToken, offset, count);
                if (!Token.TryReadFrom(data, out var token))
                {
                    throw new InvalidOperationException("Failed to decrypt token: invalid token.");
                }
                return new ValueTask<Token>(token);
            }
            finally
            {
                _decryptorPool.Return(decryptor);
            }
        }

        public ValueTask<byte[]> EncryptTokenAsync(Token token, CancellationToken cancellationToken = default)
        {
            var encryptor = _encryptorPool.Rent();
            try
            {
                var data = token.ToByteArray();
                return new ValueTask<byte[]>(encryptor.TransformFinalBlock(data, 0, data.Length));
            }
            finally
            {
                _encryptorPool.Return(encryptor);
            }
        }

        public void Dispose()
        {
            if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
            {
                _alg.Dispose();
            }
        }
    }
}