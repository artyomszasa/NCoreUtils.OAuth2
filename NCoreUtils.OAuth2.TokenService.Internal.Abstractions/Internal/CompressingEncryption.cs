using System;
using System.Buffers;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2.Internal
{
    public class CompressingEncryption : ITokenEncryption
    {
        private const int MinBufferSize = 8 * 1024;

        private const int MaxBufferSize = 8 * 8 * 1024;

        private readonly ITokenEncryption _encryption;

        public CompressingEncryption(ITokenEncryption encryption)
            => _encryption = encryption ?? throw new ArgumentNullException(nameof(encryption));

        public ValueTask<Token> DecryptTokenAsync(byte[] encryptedToken, int offset, int count, CancellationToken cancellationToken = default)
        {
            // FIXME: validate input...
            for (var bufferSize = MinBufferSize; bufferSize <= MaxBufferSize; bufferSize *= 2)
            {
                var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    if (BrotliDecoder.TryDecompress(encryptedToken.AsSpan().Slice(offset, count), buffer.AsSpan(), out var size))
                    {
                        return _encryption.DecryptTokenAsync(buffer, 0, size, cancellationToken);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            throw new InvalidOperationException($"Decrypting token requires buffer larger than {MaxBufferSize}.");
        }

        public async ValueTask<byte[]> EncryptTokenAsync(Token token, CancellationToken cancellationToken = default)
        {
            var encrypted = await _encryption.EncryptTokenAsync(token, cancellationToken);
            var maxSize = BrotliEncoder.GetMaxCompressedLength(encrypted.Length);
            var buffer = ArrayPool<byte>.Shared.Rent(maxSize);
            try
            {
                if (!BrotliEncoder.TryCompress(encrypted.AsSpan(), buffer.AsSpan(), out var size))
                {
                    throw new InvalidOperationException("Should never happen.");
                }
                return buffer[..size];
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}