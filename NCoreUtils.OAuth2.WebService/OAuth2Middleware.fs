module NCoreUtils.OAuth2.OAuth2Middleware

open System.Linq
open System.ComponentModel
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.AspNetCore
open NCoreUtils.OAuth2.Data
open NCoreUtils.Data
open NCoreUtils.Linq
open Newtonsoft.Json

let inline private (|Eq|_|) (value : string) (ci : CaseInsensitive) =
  match CaseInsensitive value = ci with
  | true -> Some ()
  | _    -> None


// common

type private OAuth2ErrorConverter () =
  inherit JsonConverter ()
  override __.CanConvert ``type`` = ``type`` = typeof<OAuth2Error>
  override __.WriteJson (writer, value, _serializer) =
    match value with
    | null -> writer.WriteNull ()
    | :? OAuth2Error as error -> OAuth2Error.stringify error |> writer.WriteValue
    | _ -> invalidOp "should never happen"
  override __.ReadJson (reader, _objectType, _existingValue, _serializer) =
    match reader.TokenType with
    | JsonToken.String  -> System.Enum.Parse<OAuth2Error> (reader.Value :?> string) |> box
    | JsonToken.Integer -> System.Convert.ToInt32 reader.Value |> enum<OAuth2Error> |> box
    | token -> JsonSerializationException (sprintf "Unable to convert %A to OAuth2Error" token) |> raise

type OAuth2ErrorResult = {
  [<JsonProperty("error"); JsonConverter(typeof<OAuth2ErrorConverter>)>]
  Error            : OAuth2Error
  [<JsonProperty("error_description")>]
  ErrorDescription : string }

type AppIdBinder (httpContextAccessor : IHttpContextAccessor, repo : IDataRepository<ClientApplication>, logger : ILogger<AppIdBinder>) =

  static let (|SomeHost|NoHost|) (input : HostString) =
    match input.HasValue with
    | true -> SomeHost input.Value
    | _    -> NoHost

  static let invalidAppIdForDomain () : ClientApplication =
    oauth2errorf OAuth2Error.InvalidRequest "%s: invalid host" OAuth2ErrorMessages.invalidClientApplication

  static let invalidAppIdNoHost () : Async<obj> =
    oauth2errorf OAuth2Error.InvalidRequest "%s: unable to retrieve host" OAuth2ErrorMessages.invalidClientApplication

  interface IValueBinder with
    member __.AsyncBind (_, _) =
      match httpContextAccessor.HttpContext with
      | null        -> invalidOp "Unable to access http context." // oauth2error OAuth2Error.InvalidRequest OAuth2ErrorMessages.invalidClientApplication
      | httpContext ->
      match httpContext.Request.Host with
      | NoHost        -> invalidAppIdNoHost ()
      | SomeHost host ->
        repo.Items
          |> Q.filter (fun ca -> ca.Domains.Any(fun domain -> domain.DomainName = host))
          |> Q.asyncTryFirst
          >>| Option.defaultWith invalidAppIdForDomain
          >>* (fun app -> logger.LogDebug("Successfully resolved client application for {0} => {1}.", host, app.Name) |> async.Return)
          >>| (fun app -> box app.Id)



type TokenServices = {
  EncryptionProvider : IEncryptionProvider
  Core               : OAuth2Core }

// password endpoint

type PasswordParameters = {
  [<ParameterBinder(typeof<AppIdBinder>)>]
  AppId    : int
  [<ParameterName(OAuth2Parameters.Username)>]
  Username : string
  [<ParameterName(OAuth2Parameters.Password)>]
  Password : string
  [<DefaultValue(null)>]
  [<ParameterName(OAuth2Parameters.Scope)>]
  Scopes   : string }

let password httpContext (services : TokenServices) (parameters : PasswordParameters) =
  services.Core.AuthenticateByPasswordAsync (parameters.AppId, parameters.Username, parameters.Password, parameters.Scopes, services.EncryptionProvider)
  >>= json httpContext

// refreshToken endpoint

type RefreshTokenParameters = {
  [<ParameterName(OAuth2Parameters.RefreshToken)>]
  RefreshToken : string }

let refreshToken httpContext (services : TokenServices) (parameters : RefreshTokenParameters) =
  services.Core.RefreshTokenAsync (parameters.RefreshToken, services.EncryptionProvider)
  >>= json httpContext

// code endpoint

type CodeParameters = {
  [<ParameterBinder(typeof<AppIdBinder>)>]
  AppId       : int
  [<ParameterName(OAuth2Parameters.RedirectUri)>]
  Redirecturi : string
  [<ParameterName(OAuth2Parameters.Code)>]
  Code        : string }

let code httpContext (services : TokenServices) (parameters : CodeParameters) =
  services.Core.AuthenticateByCodeAsync (parameters.AppId, parameters.Redirecturi, parameters.Code, services.EncryptionProvider)
  >>= json httpContext

// error "endpoint"
let error httpContext (error : OAuth2ErrorResult) =
  json httpContext error

let errorMessage httpContext err message =
  { Error = err
    ErrorDescription = message }
  |> error httpContext

let token httpContext getParameter =
  match getParameter OAuth2Parameters.GrantType >>= List.tryHead |> Option.map CaseInsensitive with
  | Some (Eq OAuth2GrantTypes.Password)     -> HttpContext.asyncBindAndExecute httpContext getParameter password
  | Some (Eq OAuth2GrantTypes.RefreshToken) -> HttpContext.asyncBindAndExecute httpContext getParameter refreshToken
  | Some (Eq OAuth2GrantTypes.Code)         -> HttpContext.asyncBindAndExecute httpContext getParameter code
  | Some _ -> OAuth2ErrorMessages.invalid OAuth2Parameters.GrantType |> errorMessage httpContext OAuth2Error.InvalidRequest
  | _      -> OAuth2ErrorMessages.missing OAuth2Parameters.GrantType |> errorMessage httpContext OAuth2Error.InvalidRequest

let inline private getErrorAndMessage e =
  match e with
  | OAuth2Exception (error, message) -> error,                      message
  | MissingParameterException name   -> OAuth2Error.InvalidRequest, OAuth2ErrorMessages.missing name
  | exn                              -> OAuth2Error.ServerError,    exn.Message

let private execute asyncNext getParameter httpContext =
  match HttpContext.path httpContext with
  | [ Eq "token" ] ->
    async {
      try       do! token httpContext getParameter
      with e -> do! getErrorAndMessage e |> ((<||) (errorMessage httpContext))
    }
  | _ -> asyncNext

let run httpContext asyncNext =
  match HttpContext.httpMethod httpContext with
  | HttpGet     -> execute asyncNext (HttpContext.tryQueryParameters httpContext) httpContext
  | HttpPost    -> execute asyncNext (HttpContext.tryFormParameters  httpContext) httpContext
  | HttpOptions -> asyncNext // FIXME: CORS
  | _           -> asyncNext
  // match path httpContext with
  // | [ Eq "token" ] -> token httpContext

