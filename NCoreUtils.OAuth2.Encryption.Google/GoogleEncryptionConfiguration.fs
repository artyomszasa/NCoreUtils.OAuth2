namespace NCoreUtils.OAuth2

[<CLIMutable>]
type GoogleEncryptionConfiguration = {
  ProjectId  : string
  LocationId : string
  KeyRingId  : string
  KeyId      : string }