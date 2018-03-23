[<AutoOpen>]
module NCoreUtils.OAuth2.OAuth2CoreExt

open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.OAuth2.Data
open System

let inline private oauth2ResultFromAccessToken (accessToken : Token) encryptedAccessToken =
  { AccessToken  = encryptedAccessToken
    TokenType    = "Bearer"
    ExpiresIn    = (accessToken.ExpiresAt - accessToken.IssuedAt).TotalSeconds |> round |> int
    RefreshToken = null }

let inline private logAccessTokenIssued (logger : ILogger) (accessToken : Token) =
  logger.LogTrace("Issued new access token for user #\"{0}\" from refresh token.", accessToken.Id)
  async.Zero ()

let inline private guidToByteArray (guid : Guid) = guid.ToByteArray ()

type OAuth2Core with
  member this.RefreshTokenAsync (refreshToken : string, encryptionProvider : IEncryptionProvider) =
    let token = async {
      try
        this.Logger.LogTrace ("Decrypting refresh token: \"{0}\".", refreshToken)
        return! encryptionProvider.DecryptTokenFromBase64 refreshToken
      with exn ->
        this.Logger.LogTrace (exn, "Failed to decrypt refresh token: \"{0}\".", refreshToken)
        return raise <| OAuth2Exception (OAuth2Error.InvalidGrant, OAuth2ErrorMessages.invalidRefreshToken, exn) }
    token
      >>= this.RefreshTokenAsync
      >>* logAccessTokenIssued this.Logger
      >>+ encryptionProvider.EncryptTokenToBase64
      >>| ((<||) oauth2ResultFromAccessToken)

  member this.AuthenticateByPasswordAsync (clientApplicationId, username, password, scopes, encryptionProvider) =
    this.AuthenticateByPasswordAsync (clientApplicationId, username, password, scopes)
    >>= ((<||) (OAuth2Response.asyncFromTokens encryptionProvider))

  member this.AuthenticateByCodeAsync (clientApplicationId, redirectUri, code, encryptionProvider : IEncryptionProvider) =
    Convert.FromBase64String code
    |>  encryptionProvider.Decrypt
    >>| (Guid : byte[] -> Guid)
    >>= (fun codeGuid -> this.AuthenticateByCodeAsync (clientApplicationId, redirectUri, codeGuid))
    >>= ((<||) (OAuth2Response.asyncFromTokens encryptionProvider))

  member this.CreateAuthorizationCodeByPasswordAsync (clientApplicationId, redirectUri, username, password, scopes, encryptionProvider : IEncryptionProvider) =
    this.CreateAuthorizationCodeByPasswordAsync (clientApplicationId, redirectUri, username, password, scopes)
    >>| guidToByteArray
    >>= encryptionProvider.Encrypt
    >>| Convert.ToBase64String


