namespace NCoreUtils.OAuth2

open System.Runtime.InteropServices
open System.Threading.Tasks
open NCoreUtils
open NCoreUtils.AspNetCore.Authentication.OpenId
open NCoreUtils.Authentication
open FSharp.Control

[<AutoOpen>]
module private Adapter =

  let ofAsyncEnumerable (enumerable : System.Collections.Generic.IAsyncEnumerable<_>) =
    { new IAsyncEnumerable<_> with
        member __.GetEnumerator () =
          let enumerator = enumerable.GetAsyncEnumerator ()
          { new IAsyncEnumerator<_> with
              member __.MoveNext () = async {
                let! next = Async.VAdapt (fun _ -> enumerator.MoveNextAsync())
                return
                  match next with
                  | true -> Some enumerator.Current
                  | _    -> None }
              member __.Dispose () =
                enumerator.DisposeAsync().AsTask().Wait()
          }
    }

  let toAsyncEnumerable (enumerable : IAsyncEnumerable<_>) =
    { new System.Collections.Generic.IAsyncEnumerable<_> with
        member __.GetAsyncEnumerator cancellationToken =
          let enumerator = enumerable.GetEnumerator ()
          let current = ref Unchecked.defaultof<_>
          { new System.Collections.Generic.IAsyncEnumerator<_> with
              member __.Current = !current
              member __.MoveNextAsync () =
                let computation = async {
                  let! next = enumerator.MoveNext ()
                  return
                    match next with
                    | Some v ->
                      current := v
                      true
                    | None ->
                      current := Unchecked.defaultof<_>
                      false }
                Async.StartAsTask (computation, cancellationToken = cancellationToken)
                |> ValueTask<bool>
              member __.DisposeAsync () =
                enumerator.Dispose ()
                Unchecked.defaultof<_>
          }
    }

type OpenIdWithScopesAuthenticationTicketEncoder<'TOpenIdClientConfiguration when 'TOpenIdClientConfiguration :> OpenIdClientConfiguration> =
  inherit OpenIdAuthenticationTicketEncoder<NCoreUtils.OAuth2.OpenIdUserInfo, 'TOpenIdClientConfiguration>

  new (configuration, loggerFactory, [<Optional;DefaultParameterValue(null:IUsernameFormatter)>] usernameFormatter) =
    { inherit OpenIdAuthenticationTicketEncoder<NCoreUtils.OAuth2.OpenIdUserInfo, 'TOpenIdClientConfiguration>(configuration, loggerFactory, usernameFormatter) }

  override this.GetClaimsAsync user =
    let source = base.GetClaimsAsync user |> ofAsyncEnumerable
    let roles =
      let roleType = this.RoleClaimType
      user.Scopes |> Seq.map (fun scope -> ClaimDescriptor(roleType, scope)) |> AsyncSeq.ofSeq
    AsyncSeq.append source roles |> toAsyncEnumerable
