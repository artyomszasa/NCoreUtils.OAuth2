namespace NCoreUtils.OAuth2

open System
open System.Text.RegularExpressions
open Microsoft.AspNetCore.Authentication
open NCoreUtils
open NCoreUtils.AspNetCore
open NCoreUtils.OAuth2.Data
open System.Security.Claims

type InternalAuthenticationSchemeOtions () =
  inherit AuthenticationSchemeOptions ()

type InternalAuthenticationHandler (options, loggerFactory, encoder, clock, encryptionProvider) =
  inherit AuthenticationHandler<InternalAuthenticationSchemeOtions> (options, loggerFactory, encoder, clock)

  //FIXME: merge with OAuth2Middleware
  static let resGetBearerToken =
    let bearerRegex = Regex ("^Bearer\\s+(.*)$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase ||| RegexOptions.CultureInvariant)
    fun authorizationString ->
      match bearerRegex.Match authorizationString with
      | m when m.Success -> Ok m.Groups.[1].Value
      | _                -> Error <| sprintf "Unable to extract bearer token from authorization string: %s" authorizationString

  //FIXME: merge with OAuth2Middleware
  static let asyncResDecryptToken (encryptionProvider : IEncryptionProvider) encryptedToken =
    let handleResult tokenResult =
      match tokenResult with
      | Choice1Of2 token       -> Ok token
      | Choice2Of2 (exn : exn) -> Error <| sprintf "Unable to decrypt token = %s, error = %s" encryptedToken exn.Message
    encryptionProvider.DecryptTokenFromBase64 encryptedToken
    |>  Async.Catch
    >>| handleResult

  //FIXME: merge with OAuth2Middleware
  static let resEnsureNonExpiredToken (token : Token) =
    match token.ExpiresAt <= DateTimeOffset.Now with
    | true -> Error <| sprintf "Token expired at %A" token.ExpiresAt
    | _    -> Ok token

  member private this.AsyncHandleAuthenticate () =
    let toAutheticationResult result =
      match result with
      | Ok (token : Token) ->
        let claims =
          [ for scope in token.Scopes do
              yield Claim (ClaimTypes.Role, scope.ToLowerString ())
            yield Claim (ClaimTypes.Sid, token.Id)
            yield Claim (ClaimTypes.Expiration, token.ExpiresAt.ToString "o", ClaimValueTypes.DateTime) ]
        let identity  = ClaimsIdentity (claims, "internal", ClaimTypes.Name, ClaimTypes.Role)
        let principal = ClaimsPrincipal identity
        let ticket    = AuthenticationTicket (principal, AuthenticationProperties (ExpiresUtc = Nullable.mk token.ExpiresAt, IssuedUtc = Nullable.mk token.IssuedAt), "internal")
        AuthenticateResult.Success ticket
      | Error (message : string) ->
        AuthenticateResult.Fail message
    this.Context
    |> HttpContext.requestHeaders
    // get authorization header
    |> Headers.resGetFirst Headers.Authorization
    // get bearer token from authorization header
    >>=  resGetBearerToken
    // wrap into async
    |>   async.Return
    // decrypt token
    >>=! asyncResDecryptToken encryptionProvider
    // ensure token has not been expired
    >>!= resEnsureNonExpiredToken
    // convert to ticket
    >>| toAutheticationResult

  override this.HandleAuthenticateAsync () = Async.StartAsTask (this.AsyncHandleAuthenticate (), cancellationToken = this.Context.RequestAborted)