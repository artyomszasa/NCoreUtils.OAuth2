using System;
using System.Buffers;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2.Internal
{
    #if NETSTANDARD2_1

    public class CompressingEncryption : ITokenEncryption
    {
        private readonly ITokenEncryption _encryption;

        public CompressingEncryption(ITokenEncryption encryption)
            => _encryption = encryption ?? throw new ArgumentNullException(nameof(encryption));

        public ValueTask<Token> DecryptTokenAsync(byte[] encryptedToken, CancellationToken cancellationToken = default)
        {
            // FIXME: minimize buffer!!!!
            var buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
            try
            {
                // var input = decoder.Decompress(encryptedToken.AsSpan(), buffer.AsSpan(), )
                if (!BrotliDecoder.TryDecompress(encryptedToken.AsSpan(), buffer.AsSpan(), out var size))
                {
                    throw new InvalidOperationException("CompressionEncryption.DecryptTokenAsync: WIP");
                }
                return _encryption.DecryptTokenAsync(buffer[..size], cancellationToken);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
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
                    throw new InvalidOperationException("Shpuld never happen.");
                }
                return buffer[..size];
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    #endif
}