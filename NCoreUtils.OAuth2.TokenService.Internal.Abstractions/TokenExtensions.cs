using System.IO;

namespace NCoreUtils.OAuth2;

public static class TokenExtensions
{
    public static byte[] ToByteArray(this Token token)
    {
        using var buffer = new MemoryStream(8192);
        using var writer = new BinaryWriter(buffer);
        token.WriteTo(writer);
        writer.Flush();
        return buffer.ToArray();
    }
}