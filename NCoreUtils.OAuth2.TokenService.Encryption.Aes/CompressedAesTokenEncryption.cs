using NCoreUtils.OAuth2.Internal;

namespace NCoreUtils.OAuth2;

public class CompressedAesTokenEncryption(AesTokenEncryptionConfiguration configuration)
    : CompressingEncryption(new AesTokenEncryption(configuration))
{ }