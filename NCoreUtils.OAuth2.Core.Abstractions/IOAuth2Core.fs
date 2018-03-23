namespace NCoreUtils.OAuth2

open System
open Microsoft.Extensions.Logging
open NCoreUtils.OAuth2.Data

type IOAuth2Core =

  abstract Logger : ILogger

  abstract CreateAuthorizationCodeByPasswordAsync
    :  clientApplicationId:int
    *  redirectUri:string
    *  username:string
    *  password:string
    *  scopes:string
    -> Async<Guid>

  abstract AuthenticateByPasswordAsync
    :  clientApplicationId:int
    *  username:string
    *  password:string
    *  scopes:string
    -> Async<Token * Token option>

  abstract AuthenticateByCodeAsync
    :  clientApplicationId:int
    *  redirectUri:string
    *  authorizationCodeGuid:Guid
    -> Async<Token * Token option>

  abstract RefreshTokenAsync : refreshToken:Token -> Async<Token>