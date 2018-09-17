namespace NCoreUtils.OAuth2

open System
open Newtonsoft.Json
open NCoreUtils.Authentication.OpenId

type OpenIdUserInfo () =
  inherit OpenIdUserInfoReponse ()
  [<JsonProperty("scope")>]
  member val Scopes = Array.empty : string[] with get, set
  [<JsonProperty("expires_at")>]
  member val ExpiresAt = Nullable<int64> () with get, set