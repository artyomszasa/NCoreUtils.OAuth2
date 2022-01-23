using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2
{
    public class CompressedAesTokenEncryption : CompressingEncryption
    {
        public CompressedAesTokenEncryption(AesTokenEncryptionConfiguration configuration)
            : base(new AesTokenEncryption(configuration))
        { }
    }
}