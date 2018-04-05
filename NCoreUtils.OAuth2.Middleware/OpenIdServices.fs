namespace NCoreUtils.OAuth2

open Microsoft.Extensions.Logging
open NCoreUtils.Data
open NCoreUtils.OAuth2.Data

[<NoEquality; NoComparison>]
type OpenIdServices = {
  EncryptionProvider : IEncryptionProvider
  UserRepository     : IDataRepository<User, int>
  Logger             : ILogger<OpenIdServices> }

