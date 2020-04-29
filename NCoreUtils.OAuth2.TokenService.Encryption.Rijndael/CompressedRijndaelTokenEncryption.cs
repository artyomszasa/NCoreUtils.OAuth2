using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    #if NETSTANDARD2_1

    public class CompressedRijndaelTokenEncryption : CompressingEncryption
    {
        public CompressedRijndaelTokenEncryption(RijndaelTokenEncryptionConfiguration configuration)
            : base(new RijndaelTokenEncryption(configuration))
        { }
    }

    #endif
}