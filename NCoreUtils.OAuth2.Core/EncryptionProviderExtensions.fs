namespace NCoreUtils.OAuth2

open System
open System.IO
open System.IO.Compression
open System.Text
open NCoreUtils
open NCoreUtils.OAuth2.Data

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module EncryptionProviderExt =

  let private utf8 = UTF8Encoding false

  type IEncryptionProvider with
    member this.EncryptToken (token : Token) =
      let plainData =
        use buffer = new MemoryStream ()
        do
          use gzip   = new GZipStream (buffer, CompressionLevel.Optimal, true)
          use writer = new BinaryWriter (gzip, utf8, true)
          Token.writeTo writer token
          writer.Flush ()
        buffer.ToArray ()
      this.Encrypt plainData

    member this.EncryptTokenToBase64 (token : Token) = async {
      let! cipherData = this.EncryptToken token
      return Convert.ToBase64String cipherData }

    member this.DecryptToken (cipherData : byte[]) = async {
      let! plainData = this.Decrypt cipherData
      let token =
        use buffer = new MemoryStream (plainData, false)
        use gzip   = new GZipStream (buffer, CompressionMode.Decompress, true)
        use reader = new BinaryReader (gzip, utf8, true)
        Token.readFrom reader
      return token }

    member this.DecryptTokenFromBase64 (cipherDataString : string) =
      this.DecryptToken <| Convert.FromBase64String cipherDataString

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module OAuth2Response =

  let inline private fromEncryptedTokens expiresIn encryptedAccessToken encryptedRefreshToken =
    { AccessToken  = encryptedAccessToken
      TokenType    = "Bearer"
      ExpiresIn    = expiresIn
      RefreshToken = encryptedRefreshToken }

  let inline private encryptIfNotNull (encryptionProvider : IEncryptionProvider) token =
    match token with
    | None       -> async.Return null
    | Some token -> encryptionProvider.EncryptTokenToBase64 token

  let asyncFromTokens (encryptionProvider : IEncryptionProvider) accessToken refreshToken =
    encryptionProvider.EncryptTokenToBase64 accessToken
    >>+ (fun _ -> encryptIfNotNull encryptionProvider refreshToken)
    >>| ((<||)) (Token.expiresIn accessToken |> fromEncryptedTokens)

