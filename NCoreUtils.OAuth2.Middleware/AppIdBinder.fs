namespace NCoreUtils.OAuth2

open System.Linq
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.Data
open NCoreUtils.Linq
open NCoreUtils.OAuth2.Data

type internal AppIdBinder (httpContextAccessor : IHttpContextAccessor, repo : IDataRepository<ClientApplication>, logger : ILogger<AppIdBinder>) =

  static let (|SomeHost|NoHost|) (input : HostString) =
    match input.HasValue with
    | true -> SomeHost input.Value
    | _    -> NoHost

  static let invalidAppIdForDomain () : ClientApplication =
    oauth2errorf OAuth2Error.InvalidRequest "%s: invalid host" OAuth2ErrorMessages.invalidClientApplication

  static let invalidAppIdNoHost () : Async<obj> =
    oauth2errorf OAuth2Error.InvalidRequest "%s: unable to retrieve host" OAuth2ErrorMessages.invalidClientApplication

  interface IValueBinder with
    member __.AsyncBind (_, _) =
      match httpContextAccessor.HttpContext with
      | null        -> invalidOp "Unable to access http context." // oauth2error OAuth2Error.InvalidRequest OAuth2ErrorMessages.invalidClientApplication
      | httpContext ->
      match httpContext.Request.Host with
      | NoHost        -> invalidAppIdNoHost ()
      | SomeHost host ->
        repo.Items
          |> Q.filter (fun ca -> ca.Domains.Any(fun domain -> domain.DomainName = host))
          |> Q.asyncTryFirst
          >>| Option.defaultWith invalidAppIdForDomain
          >>* (fun app -> logger.LogDebug("Successfully resolved client application for {0} => {1}.", host, app.Name) |> async.Return)
          >>| (fun app -> box app.Id)
