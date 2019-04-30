namespace NCoreUtils.OAuth2

open System
open System.Diagnostics.CodeAnalysis

[<CLIMutable>]
[<ExcludeFromCodeCoverage>]
type OAuth2Configuration = {
  AccessTokenExpiry       : TimeSpan
  AuthorizationCodeExpiry : TimeSpan
  RefreshTokenExpiry      : TimeSpan }