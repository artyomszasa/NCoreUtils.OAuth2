namespace NCoreUtils.OAuth2

open System.Diagnostics.CodeAnalysis
open System.Linq
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open NCoreUtils
open NCoreUtils.Data
open NCoreUtils.Linq
open NCoreUtils.OAuth2.Data
open System.Runtime.CompilerServices

[<RequireQualifiedAccess>]
module CurrentClientApplicationResolver =

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private (|SomeHost|NoHost|) (input : HostString) =
    match input.HasValue with
    | true -> SomeHost input.Value
    | _    -> NoHost

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private invalidAppIdForDomain () : ClientApplication =
    oauth2errorf OAuth2Error.InvalidRequest "%s: invalid host" OAuth2ErrorMessages.invalidClientApplication

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let private invalidAppIdNoHost () : Async<ClientApplication> =
    oauth2errorf OAuth2Error.InvalidRequest "%s: unable to retrieve host" OAuth2ErrorMessages.invalidClientApplication


  let resolveClientApplication (logger : ILogger) (repo : IDataRepository<ClientApplication>) (httpContext : HttpContext) =
    match httpContext.Request.Host with
    | NoHost        -> invalidAppIdNoHost ()
    | SomeHost host ->
      repo.Items
        |> Q.filter (fun ca -> ca.Domains.Any(fun domain -> domain.DomainName = host))
        |> Q.asyncToResizeArray
        >>| (Seq.tryHead >> Option.defaultWith invalidAppIdForDomain)
        >>* (fun app -> logger.LogDebug("Successfully resolved client application for {0} => {1}.", host, app.Name) |> async.Return)


[<AutoOpen>]
module private AppIdBinderHelpers =
  [<ExcludeFromCodeCoverage>]
  let inline getBoxedId (app : ClientApplication) = box app.Id

type internal AppIdBinder (httpContextAccessor : IHttpContextAccessor, repo : IDataRepository<ClientApplication>, logger : ILogger<AppIdBinder>) =
  interface IValueBinder with
    member __.AsyncBind (_, _) =
      match httpContextAccessor.HttpContext with
      | null        -> invalidOp "Unable to access http context." // oauth2error OAuth2Error.InvalidRequest OAuth2ErrorMessages.invalidClientApplication
      | httpContext ->
        CurrentClientApplicationResolver.resolveClientApplication logger repo httpContext
        >>| getBoxedId
