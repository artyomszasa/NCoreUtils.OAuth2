using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    public class CompressedRijndaelTokenEncryption : CompressingEncryption
    {
        public CompressedRijndaelTokenEncryption(RijndaelTokenEncryptionConfiguration configuration)
            : base(new RijndaelTokenEncryption(configuration))
        { }
    }
}