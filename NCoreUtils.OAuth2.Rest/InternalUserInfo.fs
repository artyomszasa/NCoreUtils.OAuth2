namespace NCoreUtils.OAuth2

open Microsoft.AspNetCore.Http
open NCoreUtils
open NCoreUtils.Data
open NCoreUtils.OAuth2.Data
open System.Security.Claims

type InternalUserInfo (httpContextAccessor : IHttpContextAccessor, userRepository : IDataRepository<User, int>) =

  static let tryAuthenticatedUserId (user : ClaimsPrincipal) =
    let identity = user.Identity
    match identity.IsAuthenticated with
    | false -> None
    | _     ->
    match user.FindFirst ClaimTypes.Sid with
    | null  -> None
    | claim -> tryInt claim.Value

  let httpContext = httpContextAccessor.HttpContext
  let mutable user = None : User option
  let asyncGetUser () =
    match user with
    | Some _ -> async.Return user
    | _      -> async {
      let! user1 =
        match tryAuthenticatedUserId httpContext.User with
        | None     -> async.Return None
        | Some uid -> userRepository.AsyncLookup uid >>| Option.wrap
      user <- user1
      return user1 }

  member __.AsyncTryUser () = asyncGetUser ()

module internal InternalUserInfo =

  let asyncTryUser (uinfo : InternalUserInfo) = uinfo.AsyncTryUser ()


