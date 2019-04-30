namespace NCoreUtils.OAuth2.Data

open System.Diagnostics.CodeAnalysis
open Newtonsoft.Json

[<ExcludeFromCodeCoverage>]
type OAuth2Response = {
  [<JsonProperty("access_token")>]
  AccessToken  : string
  [<JsonProperty("token_type")>]
  TokenType    : string
  [<JsonProperty("expires_in")>]
  ExpiresIn    : int
  [<JsonProperty("refresh_token")>]
  RefreshToken : string }
