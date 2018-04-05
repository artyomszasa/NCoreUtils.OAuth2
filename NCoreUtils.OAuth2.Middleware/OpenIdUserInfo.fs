namespace NCoreUtils.OAuth2

open Newtonsoft.Json

[<CLIMutable>]
[<NoEquality; NoComparison>]
type OpenIdUserInfo = {
  [<JsonProperty("sub")>]
  Id : int
  [<JsonProperty("given_name")>]
  GivenName : string
  [<JsonProperty("family_name")>]
  FamilyName : string
  [<JsonProperty("picture")>]
  Picture : string
  [<JsonProperty("email")>]
  Email : string
  [<JsonProperty("locale")>]
  Locale : string
  [<JsonProperty("scope")>]
  Scopes : string[] }