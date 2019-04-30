[<RequireQualifiedAccess>]
module NCoreUtils.OAuth2.OAuth2Middleware

open System
open System.Diagnostics.CodeAnalysis
open System.Globalization
open System.Runtime.CompilerServices
open System.Text.RegularExpressions
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open NCoreUtils
open NCoreUtils.AspNetCore
open NCoreUtils.Data
open NCoreUtils.Logging
open NCoreUtils.OAuth2.Data
open Microsoft.Extensions.Primitives

let private date1970 = DateTimeOffset.Parse ("1970-01-01T00:00:00Z", CultureInfo.InvariantCulture)

let private resGetBearerToken =
  let bearerRegex = Regex ("^Bearer\\s+(.*)$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase ||| RegexOptions.CultureInvariant)
  fun authorizationString ->
    match bearerRegex.Match authorizationString with
    | m when m.Success -> Ok m.Groups.[1].Value
    | _                -> Error <| sprintf "Unable to extract bearer token from authorization string: %s" authorizationString

[<ExcludeFromCodeCoverage>]
let inline private resEnsureNonExpiredToken (token : Token) =
  match token.ExpiresAt <= DateTimeOffset.Now with
  | true -> Error <| sprintf "Token expired at %A" token.ExpiresAt
  | _    -> Ok token

[<ExcludeFromCodeCoverage>]
let inline private resParseTokenUserId token =
  let input = Token.id token
  match tryInt32Value input with
  | ValueSome uid -> Ok    <| struct (uid, token)
  | _             -> Error <| sprintf "Invalid token user id: %s" input

let private asyncResLookupUser (userRespository : IDataRepository<User, int>) (struct (userId, token)) =
  userRespository.AsyncLookup userId
  >>| (fun user ->
        match box user with
        | null -> Error <| sprintf "No user found for id = %d" userId
        | _    -> Ok    <| struct (user, token))

let private asyncResDecryptToken (encryptionProvider : IEncryptionProvider) encryptedToken =
  let handleResult tokenResult =
    match tokenResult with
    | Choice1Of2 token       -> Ok token
    | Choice2Of2 (exn : exn) -> Error <| sprintf "Unable to decrypt token = %s, error = %s" encryptedToken exn.Message
  encryptionProvider.DecryptTokenFromBase64 encryptedToken
  |>  Async.Catch
  >>| handleResult

[<ExcludeFromCodeCoverage>]
let inline private mkOpenIdUserInfo (struct (user : User, token : Token)) =
  NCoreUtils.OAuth2.OpenIdUserInfo (
    Sub        = user.Id.ToString (),
    GivenName  = user.GivenName,
    FamilyName = user.FamilyName,
    Picture    = null,
    Email      = user.Email,
    Locale     = null,
    UpdatedAt  = ((DateTimeOffset(user.Updated, TimeSpan.Zero) - date1970).TotalSeconds |> int64 |> Nullable.mk),
    ExpiresAt  = ((token.ExpiresAt - date1970).TotalSeconds |> int64 |> Nullable.mk),
    Scopes     = (token.Scopes |> Seq.map (fun scope -> scope.ToLowerString ()) |> Seq.toArray))

[<ExcludeFromCodeCoverage>]
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

[<CompiledName("Login")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let login (httpContext : HttpContext) (services : TokenServices) (parameters : LoginParameters) = async {
  try
    let! code =
      services.Core.CreateAuthorizationCodeByPasswordAsync (
        parameters.AppId,
        parameters.RedirectUri,
        parameters.Username,
        parameters.Password,
        parameters.Scopes,
        services.EncryptionProvider)
    do
      let response = httpContext.Response
      response.StatusCode <- 302
      let uri =
        let args = sprintf "code=%s" <| Uri.EscapeDataString code
        if parameters.RedirectUri.Contains "?"
          then parameters.RedirectUri + "&" + args
          else parameters.RedirectUri + "?" + args
      response.Headers.Add ("Location", StringValues uri)
  with e ->
    do
      let uri =
        let args =
          let (err, msg) =
            match e with
            | OAuth2Exception (err, desc) -> err,                     desc
            | _                           -> OAuth2Error.ServerError, e.Message
          sprintf "error=%s&error_description=%s" (Uri.EscapeDataString <| OAuth2Error.stringify err) (Uri.EscapeDataString msg)
        if parameters.RedirectUri.Contains "?"
          then parameters.RedirectUri + "&" + args
          else parameters.RedirectUri + "?" + args
      let response = httpContext.Response
      response.StatusCode <- 302
      response.Headers.Add ("Location", StringValues uri) }


// error "endpoint"
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let private error (httpContext : HttpContext) (error : OAuth2ErrorResult) =
  httpContext.Response.StatusCode <- 400
  json httpContext error

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let private errorMessage httpContext err message =
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
  let sendResult infoResult =
    match infoResult with
    | Error message ->
      // if error has happened --> log error, send 401
      debug services.Logger message
      send401 httpContext
      async.Zero ()
    | Ok (info : OpenIdUserInfo) ->
      debugf services.Logger "Sending OpenID user info for User[Id = %s, Email = %s]" info.Sub info.Email
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

[<ExcludeFromCodeCoverage>]
let inline private getErrorAndMessage e =
  match e with
  | OAuth2Exception (error, message) -> error,                      message
  | MissingParameterException name   -> OAuth2Error.InvalidRequest, OAuth2ErrorMessages.missing name
  | exn                              -> OAuth2Error.ServerError,    exn.Message

let private tokenSafe httpContext getParameter = async {
  try       do! token httpContext getParameter
  with e ->
    let logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger "NCoreUtils.OAuth2.OAuth2Middleware"
    logger.LogDebug (e, "exception thrown while executing token endpoint.")
    do! getErrorAndMessage e |> ((<||) (errorMessage httpContext)) }

/// Performes route matching
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let private execute asyncNext getParameter httpContext =
  match HttpContext.path httpContext with
  | [ Eq "token"  ] -> tokenSafe httpContext getParameter
  | [ Eq "openid" ] -> HttpContext.asyncBindAndExecute httpContext getParameter openid
  | [ Eq "login"  ] -> HttpContext.asyncBindAndExecute httpContext getParameter login
  | _ -> asyncNext

[<CompiledName("Run")>]
let run httpContext asyncNext =
  match HttpContext.httpMethod httpContext with
  | HttpGet     -> execute asyncNext (HttpContext.tryQueryParameters httpContext) httpContext
  | HttpPost    -> execute asyncNext (HttpContext.tryFormParameters  httpContext) httpContext
  | HttpOptions -> asyncNext // FIXME: CORS
  | _           -> asyncNext
