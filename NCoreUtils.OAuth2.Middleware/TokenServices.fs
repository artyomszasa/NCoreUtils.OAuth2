namespace NCoreUtils.OAuth2

[<NoEquality; NoComparison>]
type TokenServices = {
  EncryptionProvider : IEncryptionProvider
  Core               : IOAuth2Core }

