namespace NCoreUtils.OAuth2

open System.Runtime.CompilerServices
open System.Threading.Tasks
open FSharp.Control
open NCoreUtils
open NCoreUtils.Authentication
open NCoreUtils.Data
open NCoreUtils.Linq
open NCoreUtils.OAuth2.Data

type private ToAsyncEnumerator<'T> (enumerator : IAsyncEnumerator<'T>, cancellationToken) =
  let mutable current = Unchecked.defaultof<_>
  interface System.Collections.Generic.IAsyncEnumerator<'T> with
    member __.Current = current
    member __.MoveNextAsync () =
      let computation = async {
        let! next = enumerator.MoveNext ()
        return
          match next with
          | Some value ->
            current <- value
            true
          | _ ->
            current <- Unchecked.defaultof<_>
            false }
      ValueTask<bool> (Async.StartAsTask (computation, cancellationToken = cancellationToken))
    member __.DisposeAsync () =
      current <- Unchecked.defaultof<_>
      enumerator.Dispose ()
      Unchecked.defaultof<_>

type OAuth2UserManager (userRepository : IDataRepository<User>, currentClientApplication : CurrentClientApplication) =

  static let toAsyncEnumerable (source: IAsyncEnumerable<_>) =
    { new System.Collections.Generic.IAsyncEnumerable<_> with
        member this.GetAsyncEnumerator cancellationToken = ToAsyncEnumerator<_> (source.GetEnumerator (), cancellationToken) :> _
    }


  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member private __.AsyncFindByEmail email =
    let appId = currentClientApplication.Id;
    userRepository.Items
    |> Q.filter (fun u -> u.Email = email && u.ClientApplicationId = appId)
    |> Q.asyncToResizeArray
    >>| (fun us -> us |> Seq.head :> IUser<_>)

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member private __.AsyncGetPermissionsAsync (user : IUser<_>) =
    let userId = user.Id
    userRepository.Items
      |> Q.filter  (fun u  -> u.Id = userId)
      |> Q.collect (fun u  -> u.Permissions :> _)
      |> Q.map     (fun up -> up.Permission.Name)
      |> Q.toAsyncSeq

  interface IUserManager<int> with
    member this.FindByEmailAsync (email, cancellationToken) =
      Async.StartAsTask (this.AsyncFindByEmail email, cancellationToken = cancellationToken)
      |> ValueTask<IUser<_>>

    member this.GetPermissionsAsync user =
        this.AsyncGetPermissionsAsync user
        |> toAsyncEnumerable
