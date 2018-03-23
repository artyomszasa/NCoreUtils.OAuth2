[<RequireQualifiedAccess>]
module NCoreUtils.OAuth2.OAuth2Middleware

open NCoreUtils
open NCoreUtils.AspNetCore

let password httpContext (services : TokenServices) (parameters : PasswordParameters) =
  services.Core.AuthenticateByPasswordAsync (parameters.AppId, parameters.Username, parameters.Password, parameters.Scopes, services.EncryptionProvider)
  >>= json httpContext

let refreshToken httpContext (services : TokenServices) (parameters : RefreshTokenParameters) =
  services.Core.RefreshTokenAsync (parameters.RefreshToken, services.EncryptionProvider)
  >>= json httpContext

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
