namespace NCoreUtils.OAuth2

open System.Runtime.CompilerServices

type OAuth2Error =
  | InvalidRequest          = 0
  | UnauthorizedClient      = 1
  | AccessDenied            = 2
  | UnsupportedResponseType = 3
  | InvalidScope            = 4
  | InvalidGrant            = 5
  | ServerError             = 6
  | TemporarilyUnavailable  = 7

[<Extension>]
[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module OAuth2Error =

  let internal strings =
    Map.ofList
      [ OAuth2Error.InvalidRequest,          "invalid_request"
        OAuth2Error.UnauthorizedClient,      "unauthorized_client"
        OAuth2Error.AccessDenied,            "access_denied"
        OAuth2Error.UnsupportedResponseType, "unsupported_response_type"
        OAuth2Error.InvalidScope,            "invalid_scope"
        OAuth2Error.InvalidGrant,            "invalid_grant"
        OAuth2Error.ServerError,             "server_error"
        OAuth2Error.TemporarilyUnavailable,  "temporarily_unavailable" ]

  let internal errors =
    strings
    |> Map.toList
    |> List.map (fun (k, v) -> (v, k))
    |> Map.ofList

  [<CompiledName("Stringify")>]
  [<Extension>]
  let stringify error =
    Map.tryFind error strings
    |> Option.defaultValue "unknown_error"

  [<CompiledName("Parse")>]
  let parse errorString =
    Map.find errorString errors