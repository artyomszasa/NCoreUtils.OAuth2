namespace NCoreUtils.OAuth2

open System.Runtime.CompilerServices
open NCoreUtils
open NCoreUtils.Authentication
open NCoreUtils.Data
open NCoreUtils.Linq
open NCoreUtils.OAuth2.Data

type OAuth2UserManager (userRepository : IDataRepository<User>, currentClientApplication : CurrentClientApplication) =

  static let unwrapUnsafe x =
    match x with
    | Some x -> x
    | _      -> Unchecked.defaultof<_>

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member private __.AsyncFindByEmail email =
    let appId = currentClientApplication.Id;
    userRepository.Items
    |> Q.filter (fun u -> u.Email = email && u.ClientApplicationId = appId)
    |> Q.asyncTryFirst
    >>| (fun u -> unwrapUnsafe u :> IUser<_>)

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member private __.AsyncGetPermissionsAsync (user : IUser<_>) =
    let userId = user.Id
    userRepository.Items
      |> Q.filter  (fun u  -> u.Id = userId)
      |> Q.collect (fun u  -> u.Permissions :> _)
      |> Q.map     (fun up -> up.Permission.Name)
      |> Q.toAsyncSeq

  interface IUserManager<int> with
    member this.FindByEmailAsync (email, cancellationToken) = Async.StartAsTask (this.AsyncFindByEmail email, cancellationToken = cancellationToken)

    member this.GetPermissionsAsync user =
        this.AsyncGetPermissionsAsync user
        |> toAsyncEnumerable
