namespace NCoreUtils.OAuth2

open NCoreUtils

type CodeParameters = {
  [<ParameterBinder(typeof<AppIdBinder>)>]
  AppId       : int
  [<ParameterName(OAuth2Parameters.RedirectUri)>]
  Redirecturi : string
  [<ParameterName(OAuth2Parameters.Code)>]
  Code        : string }
