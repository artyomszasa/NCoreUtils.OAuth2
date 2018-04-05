[<RequireQualifiedAccess>]
module NCoreUtils.OAuth2.OAuth2Middleware

open System.Text.RegularExpressions
open NCoreUtils
open NCoreUtils.AspNetCore
open NCoreUtils.Data
open NCoreUtils.Logging
open NCoreUtils.OAuth2.Data
open System
open System.Runtime.CompilerServices

let private resGetBearerToken =
  let bearerRegex = Regex ("^Bearer\\s+(.*)$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase ||| RegexOptions.CultureInvariant)
  fun authorizationString ->
    match bearerRegex.Match authorizationString with
    | m when m.Success -> Ok m.Groups.[1].Value
    | _                -> Error <| sprintf "Unable to extract bearer token from authorization string: %s" authorizationString

let inline private resEnsureNonExpiredToken (token : Token) =
  match token.ExpiresAt <= DateTimeOffset.Now with
  | true -> Error <| sprintf "Token expired at %A" token.ExpiresAt
  | _    -> Ok token

let inline private resParseTokenUserId token =
  let input = Token.id token
  match tryInt32 input with
  | Some uid -> Ok    <| struct (uid, token)
  | _        -> Error <| sprintf "Invalid token user id: %s" input

let inline private asyncResLookupUser (userRespository : IDataRepository<User, int>) (struct (userId, token)) =
  userRespository.AsyncLookup userId
  >>| (fun user ->
        match box user with
        | null -> Error <| sprintf "No user found for id = %d" userId
        | _    -> Ok    <| struct (user, token))

let inline private asyncResDecryptToken (encryptionProvider : IEncryptionProvider) encryptedToken =
  let inline handleResult tokenResult =
    match tokenResult with
    | Choice1Of2 token       -> Ok token
    | Choice2Of2 (exn : exn) -> Error <| sprintf "Unable to decrypt token = %s, error = %s" encryptedToken exn.Message
  encryptionProvider.DecryptTokenFromBase64 encryptedToken
  |>  Async.Catch
  >>| handleResult

let inline private mkOpenIdUserInfo (struct (user : User, token : Token)) =
  { Id         = user.Id
    GivenName  = user.GivenName
    FamilyName = user.FamilyName
    Picture    = null
    Email      = user.Email
    Locale     = null
    Scopes     = token.Scopes |> Seq.map (fun scope -> scope.ToLowerString ()) |> Seq.toArray }

let inline private send401 httpContext = HttpContext.setResponseStatusCode 401 httpContext

[<CompiledName("Password")>]
let password httpContext (services : TokenServices) (parameters : PasswordParameters) =
  services.Core.AuthenticateByPasswordAsync (parameters.AppId, parameters.Username, parameters.Password, parameters.Scopes, services.EncryptionProvider)
  >>= json httpContext

[<CompiledName("RefreshToken")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let refreshToken httpContext (services : TokenServices) (parameters : RefreshTokenParameters) =
  services.Core.RefreshTokenAsync (parameters.RefreshToken, services.EncryptionProvider)
  >>= json httpContext

[<CompiledName("Code")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let code httpContext (services : TokenServices) (parameters : CodeParameters) =
  services.Core.AuthenticateByCodeAsync (parameters.AppId, parameters.Redirecturi, parameters.Code, services.EncryptionProvider)
  >>= json httpContext

// error "endpoint"
let inline private error httpContext (error : OAuth2ErrorResult) =
  json httpContext error

let inline private errorMessage httpContext err message =
  { Error = err
    ErrorDescription = message }
  |> error httpContext

[<CompiledName("TokenEndPoint")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let token httpContext getParameter =
  match getParameter OAuth2Parameters.GrantType >>= List.tryHead >>| CaseInsensitive with
  | Some (Eq OAuth2GrantTypes.Password)     -> HttpContext.asyncBindAndExecute httpContext getParameter password
  | Some (Eq OAuth2GrantTypes.RefreshToken) -> HttpContext.asyncBindAndExecute httpContext getParameter refreshToken
  | Some (Eq OAuth2GrantTypes.Code)         -> HttpContext.asyncBindAndExecute httpContext getParameter code
  | Some _ -> OAuth2ErrorMessages.invalid OAuth2Parameters.GrantType |> errorMessage httpContext OAuth2Error.InvalidRequest
  | _      -> OAuth2ErrorMessages.missing OAuth2Parameters.GrantType |> errorMessage httpContext OAuth2Error.InvalidRequest

[<CompiledName("OpenIDEndPoint")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let openid httpContext (services : OpenIdServices) (_parameters : unit) =
  let inline sendResult infoResult =
    match infoResult with
    | Error message ->
      // if error has happened --> log error, send 401
      debug services.Logger message
      send401 httpContext
      async.Zero ()
    | Ok (info : OpenIdUserInfo) ->
      debugf services.Logger "Sending OpenID user info for User[Id = %d, Email = %s]" info.Id info.Email
      json httpContext info
  HttpContext.requestHeaders httpContext
    // get authorization header
    |> Headers.resGetFirst Headers.Authorization
    // get bearer token from authorization header
    >>=  resGetBearerToken
    // wrap into async
    |>   async.Return
    // decrypt token
    >>=! asyncResDecryptToken services.EncryptionProvider
    // ensure token has not been expired
    >>!= resEnsureNonExpiredToken
    // parse user id preserving token
    >>!= resParseTokenUserId
    // lookup user by id preserving token
    >>=! asyncResLookupUser services.UserRepository
    // create OpenID user info
    >>!| mkOpenIdUserInfo
    // send either user info or 401
    >>=  sendResult

let inline private getErrorAndMessage e =
  match e with
  | OAuth2Exception (error, message) -> error,                      message
  | MissingParameterException name   -> OAuth2Error.InvalidRequest, OAuth2ErrorMessages.missing name
  | exn                              -> OAuth2Error.ServerError,    exn.Message

let private tokenSafe httpContext getParameter = async {
  try       do! token httpContext getParameter
  with e -> do! getErrorAndMessage e |> ((<||) (errorMessage httpContext)) }

/// Performes route matching
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let private execute asyncNext getParameter httpContext =
  match HttpContext.path httpContext with
  | [ Eq "token"  ] -> tokenSafe httpContext getParameter
  | [ Eq "openid" ] -> HttpContext.asyncBindAndExecute httpContext getParameter openid
  | _ -> asyncNext

[<CompiledName("Run")>]
let run httpContext asyncNext =
  match HttpContext.httpMethod httpContext with
  | HttpGet     -> execute asyncNext (HttpContext.tryQueryParameters httpContext) httpContext
  | HttpPost    -> execute asyncNext (HttpContext.tryFormParameters  httpContext) httpContext
  | HttpOptions -> asyncNext // FIXME: CORS
  | _           -> asyncNext
