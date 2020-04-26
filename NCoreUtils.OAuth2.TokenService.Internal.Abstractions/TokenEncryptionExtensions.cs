using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
#if !NETSTANDARD2_1
using Convert = NCoreUtils.PolyfillConvert;
#endif

namespace NCoreUtils.OAuth2
{
    public static class TokenEncryptionExtensions
    {
        // static string ToBase64String(ReadOnlySpan<byte> raw)
        // {
        //     var bufferSize = Math.Max(16 * 1024, (int)(Math.Ceiling((double)raw.Length / 3.0d) * 4.0d) + 4);
        //     string result;
        //     if (bufferSize == 16 * 1024)
        //     {
        //         Span<char> buffer = stackalloc char[bufferSize];
        //         Convert.TryToBase64Chars(raw, buffer, out var size, Base64FormattingOptions.None);
        //         result = buffer.Slice(0, size).ToString();
        //     }
        //     else
        //     {
        //         var buffer = new char[bufferSize];
        //         Convert.TryToBase64Chars(raw, buffer, out var size, Base64FormattingOptions.None);
        //         result = new string(buffer, 0, size);
        //     }
        //     return result;
        // }

        // public static async ValueTask<Token?> DecryptTokenFromBase64StringAsync(
        //     this ITokenEncryption encryption,
        //     string base64string,
        //     CancellationToken cancellationToken = default)
        // {
        //     var bufferSize = Math.Max(16 * 1024, (int)(Math.Ceiling((double)base64string.Length / 4.0d) * 3.0d));
        //     using var buffer = MemoryPool<byte>.Shared.Rent(bufferSize);
        //     Convert.TryFromBase64String(base64string, buffer.Memory.Span, out var size);
        //     return await encryption.DecryptTokenAsync(buffer.Memory.Slice(0, size), cancellationToken);
        // }

        // public static async ValueTask<string> EncryptTokenToBase64StringAsync(
        //     this ITokenEncryption encryption,
        //     Token token,
        //     CancellationToken cancellationToken = default)
        // {
        //     var bufferSize = 8192; // start with 16k (will be multiplied in loop).
        //     do
        //     {
        //         bufferSize *= 2;
        //         using var buffer = MemoryPool<byte>.Shared.Rent(bufferSize);
        //         var res = await encryption.TryEncryptTokenAsync(token, buffer.Memory, cancellationToken);
        //         if (res.Success)
        //         {
        //             return ToBase64String(buffer.Memory.Span.Slice(0, res.Size));
        //         }
        //     }
        //     while (bufferSize < 32 * 1024);
        //     throw new InvalidOperationException("Token is too large.");
        // }

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
}