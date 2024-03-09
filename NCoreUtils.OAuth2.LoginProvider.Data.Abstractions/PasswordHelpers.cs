using System;
using System.Text;
using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2;

public static class PasswordHelpers
{
    private static UTF8Encoding Utf8 { get; } = new(false);

    /// <summary>
    /// Computes sha512 hash as base64 string for plain-text password using the specified salt.
    /// <para>
    /// Intended usage: validating passwords.
    /// </para>
    /// </summary>
    /// <param name="password">Plain-text password.</param>
    /// <param name="salt">Salt.</param>
    /// <returns>Computed hash as base64 string.</returns>
    public static string ComputeHash(string password, string salt)
    {
        var sha512 = Sha512Helper.Rent();
        try
        {
            var input = $"{password}:{salt}";
            // try use stack allocation if input is small enough
            if (input.Length <= 2048)
            {
                Span<byte> inputBuffer = stackalloc byte[input.Length * 4];
                var inputSize = Utf8.GetBytes(input.AsSpan(), inputBuffer);
                Span<byte> buffer = stackalloc byte[8192];
                if (sha512.TryComputeHash(inputBuffer[..inputSize], buffer, out var size))
                {
                    return Convert.ToBase64String(buffer[..size], Base64FormattingOptions.None);
                }
            }
            // fallback to heap allocation
            return Convert.ToBase64String(sha512.ComputeHash(Utf8.GetBytes($"{password}:{salt}")));
        }
        finally
        {
            Sha512Helper.Return(sha512);
        }
    }

    /// <summary>
    /// Generates random salt and computes sha512 hash as base64 string for plain-text password using the generated
    /// salt.
    /// <para>
    /// Intended usage: generating initial salt/password pair.
    /// </para>
    /// </summary>
    /// <param name="password">Plain-text password</param>
    /// <returns>Generated salt and computed hash.</returns>
    public static (string Hash, string Salt) ComputeHash(string password)
    {
        var salt = GenerateSalt();
        var hash = ComputeHash(password, salt);
        return (hash, salt);
    }

    /// <summary>
    /// Generates random salt.
    /// </summary>
    /// <returns>Generated salt.</returns>
    public static string GenerateSalt()
    {
        var sha512 = Sha512Helper.Rent();
        try
        {
            var guid = Guid.NewGuid();
            // try use stack allocation
            Span<byte> source = stackalloc byte[16];
            if (guid.TryWriteBytes(source))
            {
                Span<byte> buffer = stackalloc byte[8192];
                if (sha512.TryComputeHash(source, buffer, out var size))
                {
                    return Convert.ToBase64String(buffer[..size], Base64FormattingOptions.None);
                }
            }
            // fallback to heap allocation
            return Convert.ToBase64String(sha512.ComputeHash(Guid.NewGuid().ToByteArray()));
        }
        finally
        {
            Sha512Helper.Return(sha512);
        }
    }
}