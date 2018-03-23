namespace NCoreUtils.OAuth2

open System

[<CLIMutable>]
type OAuth2Configuration = {
  AccessTokenExpiry       : TimeSpan
  AuthorizationCodeExpiry : TimeSpan
  RefreshTokenExpiry      : TimeSpan }