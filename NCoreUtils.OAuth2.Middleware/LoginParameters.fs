namespace NCoreUtils.OAuth2

open System.ComponentModel
open NCoreUtils

[<NoEquality; NoComparison>]
type LoginParameters = {
  [<ParameterBinder(typeof<AppIdBinder>)>]
  AppId       : int
  [<ParameterName(OAuth2Parameters.Username)>]
  Username    : string
  [<ParameterName(OAuth2Parameters.Password)>]
  Password    : string
  [<ParameterName(OAuth2Parameters.RedirectUri)>]
  RedirectUri : string
  [<DefaultValue(null)>]
  [<ParameterName(OAuth2Parameters.Scope)>]
  Scopes      : string }
