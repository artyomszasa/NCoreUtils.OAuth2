namespace NCoreUtils.OAuth2.Rest

open NCoreUtils
open NCoreUtils.Data
open NCoreUtils.OAuth2.Data
open NCoreUtils.AspNetCore.Rest

type MappedClientApplication = {
  Id          : int
  Name        : string
  Description : string }

type MappedUserPermission = {
  UserId       : int
  PermissionId : int }

type MappedUser = {
  Id                  : int
  Created             : int64
  Updated             : int64
  State               : State
  ClientApplicationId : int
  ClientApplication   : MappedClientApplication
  HonorificPrefix     : string
  FamilyName          : string
  GivenName           : string
  MiddleName          : string
  Email               : string
  Avatar              : File
  Permissions         : MappedUserPermission[] }

type IDefaultUserSerializer =
  inherit ISerializer<MappedUser>

type IDefaultUserCollectionSerializer =
  inherit ISerializer<MappedUser[]>

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module private MappedClientApplication =

  let ofClientApplication (app : ClientApplication) =
    { Id          = app.Id
      Name        = app.Name
      Description = app.Description }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module private MappedUser =

  let ofUser (user : User) =
    { Id                  = user.Id
      Created             = user.Created
      Updated             = user.Updated
      State               = user.State
      ClientApplicationId = user.ClientApplicationId
      ClientApplication   = MappedClientApplication.ofClientApplication user.ClientApplication
      HonorificPrefix     = user.HonorificPrefix
      FamilyName          = user.FamilyName
      GivenName           = user.GivenName
      MiddleName          = user.MiddleName
      Email               = user.Email
      Avatar              = user.Avatar
      Permissions         = user.Permissions |> Seq.mapToArray (fun up -> { UserId = up.UserId; PermissionId = up.PermissionId }) }

type RestUserSerializer (defaultSerializer : IDefaultUserSerializer) =
  interface ISerializer<User> with
    member __.AsyncSerialize (output, item) =
      defaultSerializer.AsyncSerialize (output, MappedUser.ofUser item)

type RestUserCollectionSerializer (defaultSerializer : IDefaultUserCollectionSerializer) =
  interface ISerializer<User[]> with
    member __.AsyncSerialize (output, item) =
      defaultSerializer.AsyncSerialize (output, Array.map MappedUser.ofUser item)
