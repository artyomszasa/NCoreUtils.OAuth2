namespace NCoreUtils.OAuth2

[<Struct>]
[<NoEquality; NoComparison>]
type EncryptedPassword = {
  PasswordHash : string
  Salt         : string }

[<Interface>]
type IPasswordEncryption =
  abstract EncryptPassword : unencryptedPassword:string -> EncryptedPassword