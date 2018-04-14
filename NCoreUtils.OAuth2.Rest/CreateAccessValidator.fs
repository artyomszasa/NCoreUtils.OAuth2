namespace NCoreUtils.OAuth2

open NCoreUtils.AspNetCore.Rest
open NCoreUtils.DependencyInjection
open NCoreUtils.Linq
open NCoreUtils.OAuth2.Data
open System.Linq

[<RequireQualifiedAccess>]
module RestAccessValidation =

  [<CompiledName("Create")>]
  let create =
    { new IEntityAccessValidator with
        member __.AsyncValidate (_, principal) = async.Return principal.Identity.IsAuthenticated
        member __.AsyncValidate (entity, serviceProvider, principal) = async {
          let! user0 =
            getService<InternalUserInfo> serviceProvider
            |> InternalUserInfo.asyncTryUser
          return
            match user0 with
            | None      -> false
            | Some user ->
              match entity with
              | :? User       as u -> principal.IsInRole Permissions.User.Write       && u.ClientApplicationId = user.ClientApplicationId
              | :? Permission as p -> principal.IsInRole Permissions.Permission.Write && p.ClientApplicationId = user.ClientApplicationId
              | _                  -> false }
    }

  [<CompiledName("Update")>]
  let update =
    { new IEntityAccessValidator with
        member __.AsyncValidate (_, principal) = async.Return principal.Identity.IsAuthenticated
        member __.AsyncValidate (entity, serviceProvider, principal) = async {
          let! user0 =
            getService<InternalUserInfo> serviceProvider
            |> InternalUserInfo.asyncTryUser
          return
            match user0 with
            | None      -> false
            | Some user ->
              match entity with
              | :? User as u ->
                match principal.IsInRole Permissions.User.Write with
                | true -> u.ClientApplicationId = user.ClientApplicationId // user with write permission can update another user is same app
                | _    -> u.Id = user.Id // user can update own data without any further permissions
              | :? Permission as p ->
                principal.IsInRole Permissions.Permission.Write && p.ClientApplicationId = user.ClientApplicationId // user with write permission can update permissions in same app
              | _ -> false }
    }

  [<CompiledName("Delete")>]
  let delete =
    { new IEntityAccessValidator with
        member __.AsyncValidate (_, principal) = async.Return principal.Identity.IsAuthenticated
        member __.AsyncValidate (entity, serviceProvider, principal) = async {
          let! user0 =
            getService<InternalUserInfo> serviceProvider
            |> InternalUserInfo.asyncTryUser
          return
            match user0 with
            | None      -> false
            | Some user ->
              match entity with
              | :? User       as u -> principal.IsInRole Permissions.User.Delete       && u.ClientApplicationId = user.ClientApplicationId
              | :? Permission as p -> principal.IsInRole Permissions.Permission.Delete && p.ClientApplicationId = user.ClientApplicationId
              | _ -> false }
    }

  [<CompiledName("Item")>]
  let item =
    { new IEntityAccessValidator with
        member __.AsyncValidate (_, principal) = async.Return principal.Identity.IsAuthenticated
        member __.AsyncValidate (entity, serviceProvider, principal) = async {
          let! user0 =
            getService<InternalUserInfo> serviceProvider
            |> InternalUserInfo.asyncTryUser
          return
            match user0 with
            | None      -> false
            | Some user ->
              match entity with
              | :? User as u ->
                match principal.IsInRole Permissions.User.Read with
                | true -> u.ClientApplicationId = user.ClientApplicationId // user with read permission can access another user is same app
                | _    -> u.Id = user.Id // user can read own data without any further permissions
              | :? Permission -> true // permission read access is not guarded
              | _ -> false }
    }

  [<CompiledName("List")>]
  let list =
    { new IQueryAccessValidator with
        member __.AsyncValidate (_, principal) = async.Return principal.Identity.IsAuthenticated
        member __.AsyncFilterQuery (queryable, serviceProvider, principal) = async {
          let! user0 =
            getService<InternalUserInfo> serviceProvider
            |> InternalUserInfo.asyncTryUser
          return
            match user0 with
            | None      -> queryable // Should never happen
            | Some user ->
              match queryable.ElementType with
              | t when t = typeof<User> ->
                let q =
                  let q = queryable :?> IQueryable<User>
                  match principal.IsInRole Permissions.User.Read with
                  | true -> q |> Q.filter (fun u -> u.ClientApplicationId = user.ClientApplicationId) // user with read permission can access another user is same app
                  | _    -> q |> Q.filter (fun u -> u.Id = user.Id) // user can read own data without any further permissions
                q :> IQueryable
              | _ -> queryable }
    }